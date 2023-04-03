namespace GameJam;

public partial class VillageState : GameState
{
	[Net] public float Radius { get; set; } = 500f;
	[Net] public Vector3 Position { get; set; }

	VillageHUD hud;

	public override void Initialize()
	{
		if ( Game.IsClient )
		{
			hud = HUD.Instance.AddChild<VillageHUD>();
			return;
		}

		Town.GenerateTown( 40f, 2f );
	}

	public override void Changed( GameState state )
	{
		hud?.Delete( true );
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
