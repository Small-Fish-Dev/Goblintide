using static GoblinGame.WorldMapHost;

namespace GoblinGame;

public partial class Lord : BaseCharacter
{
	public static Lord Self => (Lord)Game.LocalPawn;

	public override FactionType DefaultFaction { get; set; } = FactionType.Goblins;

	public float BaseWalkSpeed { get; set; } = 140f;
	public float WalkSpeed { get; set; } = 140f;

	public override bool BlockNav { get; set; } = true;
	public override string DamageSound => "sounds/lord/lord_hurt.sound";

	public override void Spawn()
	{
		base.Spawn();
		Faction = DefaultFaction;
		MaxHitPoints = 10f;
		HitPoints = MaxHitPoints;

		CollisionWidth = 20f;
		CollisionHeight = 40f;

		SetModel( "models/goblin/goblin.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "Player" );
		Tags.Add( "Pushable" );
		Tags.Add( Faction.ToString() );

		// Add obscured glow.
		var glow = Components.GetOrCreate<Glow>();
		glow.Enabled = true;
		glow.ObscuredColor = Color.White;
		glow.Color = Color.Transparent;

		SetMaterialGroup( "old" );
		SetBodyGroup( "crown", 1 );
	}
	public override void Kill()
	{
		if ( Goblintide.State is not VillageState )
		{
			var count = Goblintide.GoblinArmy.Count;
			for ( int i = 0; i < count; i++ )
				Goblintide.GoblinArmy.First()?.Delete();

			Goblintide.SetState<VillageState>();
			Sound.FromScreen( "sounds/ui/trumpets_failfare.sound" );
		}

		HitPoints = MaxHitPoints;
		TotalAttackers = 0;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		// Overview mode toggle.
		if ( Input.Released( InputButton.Score ) )
		{
			Overview = !Overview;
			OverviewOffset = 0;
		}

		HitPoints = Math.Min( HitPoints + 0.5f * Time.Delta, MaxHitPoints );

		SimulateController();
		SimulateAnimations();
		SimulateCommanding();
		SimulateUpgrades();

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
		}, ShowLordInfo );

		Debug.Section( "Lord Upgrades", () =>
		{
			var lord = (Lord)Game.LocalPawn;
			foreach ( var upgrade in lord.Upgrades ) Debug.Add( upgrade );
		} );
	}
}
