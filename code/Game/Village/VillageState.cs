namespace GameJam;

public partial class VillageState : GameState
{
	VillageHUD hud;

	public override void Initialize()
	{
		if ( Game.IsClient )
			hud = HUD.Instance.AddChild<VillageHUD>();

		GameMgr.Lord.PointOfInterest = GameMgr.Lord.Position;
	}

	[Event.Tick]
	private void onTick()
	{
		// Block movement if in overview mode.
		GameMgr.Lord.BlockMovement = GameMgr.Lord.Overview;
		
	}
}
