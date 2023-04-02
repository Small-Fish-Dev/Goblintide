using GameJam.UpgradeSystem;

namespace GameJam;

public partial class Lord : BaseCharacter
{
	public override float HitPoints { get; set; } = 1500f; // Debug I don't wanna die

	public override FactionType Faction { get; set; } = FactionType.Goblins;

	public float WalkSpeed => 140f;

	public override float CollisionWidth { get; set; } = 20f;
	public override float CollisionHeight { get; set; } = 40f;
	public override bool BlockNav { get; set; } = true;

	[BindComponent] public EnergyComponent Energy { get; }

	public override void Spawn()
	{
		SetModel( "models/goblin/goblin.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "Player" );
		Tags.Add( "Pushable" );
		Tags.Add( Faction.ToString() );

		Components.Create<EnergyComponent>();

		// Add obscured glow.
		var glow = Components.GetOrCreate<Glow>();
		glow.Enabled = true;
		glow.ObscuredColor = Color.White;
		glow.Color = Color.Transparent;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		// Overview mode toggle.
		if ( Input.Released( InputButton.Score ) )
			Overview = !Overview;

		SimulateController();
		SimulateAnimations();
		SimulateCommanding();

		Scale = 1.2f; //Idk why it doesn't work on Spawn()
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		SimulateController(); // Smooth push
		SimulateCamera();
		SimulateAnimations();
	}

	[ConVar.Client( "gdbg_lord" )] private static bool ShowLordInfo { get; set; } = true;

	[Debug.Draw]
	private static void DebugDraw()
	{
		Debug.Section( "Lord", () =>
		{
			var lord = (Lord)Game.LocalPawn;
			Debug.Value( "Faction", lord.Faction );
			Debug.Value( "Hit Points", lord.HitPoints );
			Debug.Value( "Energy", lord.Energy.Value );
		}, ShowLordInfo );
	}
}
