namespace GameJam;

public partial class VillageState : GameState
{
	VillageHUD hud;

	public override void Initialize()
	{
		if ( Game.IsClient )
			hud = HUD.Instance.AddChild<VillageHUD>();
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
