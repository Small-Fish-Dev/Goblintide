namespace GameJam;

public partial class VillageState : GameState
{
	VillageHUD hud;

	public override void Initialize()
	{
		if ( Game.IsClient )
			hud = HUD.Instance.AddChild<VillageHUD>();
		else
			Town.GenerateTown( 40f, 2f );
	}

	public override void Changed( GameState state )
	{
		GameMgr.Instance.CurrentTown.DeleteTown();
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
