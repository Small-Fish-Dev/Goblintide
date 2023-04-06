using GameJam.UI;

namespace GameJam;

public partial class RaidingState : GameState
{

	RaidingHUD hud;
	static RaidingState instance;

	public override void Initialize()
	{
		instance = this;

		if ( Game.IsClient )
		{
			hud = HUD.Instance.AddChild<RaidingHUD>();
			WorldMap.Delete();
			SkillTree.Delete();
			return;
		}
		else
		{
			GameMgr.GoblinArmyEnabled( false );
			GameMgr.PlaceGoblinArmy( false );
			GameMgr.Lord.Position = GameMgr.CurrentTown.Position + Vector3.Backward * ( GameMgr.CurrentTown.TownRadius + 400f );
		}
	}

	public override void Changed( GameState state )
	{
		hud?.Delete( true );

		if ( Game.IsServer )
		{
			foreach ( var entity in Entity.All.OfType<BaseStructure>() )
				entity.Delete();

			Log.Info( "Autosaving..." );
			GameTask.RunInThreadAsync( async () =>
			{
				await GameMgr.GenerateSave( true );
				Log.Info( "Finished autosaving" );
			} );
		}
	}

	[Event.Tick]
	private void onTick()
	{
		if ( GameMgr.Lord == null )
			return;

		// Block movement if in overview mode.
		GameMgr.Lord.BlockMovement = GameMgr.Lord.Overview;
	}
}
