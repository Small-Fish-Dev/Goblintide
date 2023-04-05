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
			return;
		}
	}

	public override void Changed( GameState state )
	{
		hud?.Delete( true );

		if ( Game.IsServer )
		{
			foreach ( var entity in Entity.All.OfType<BaseStructure>() )
				entity.Delete();
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
