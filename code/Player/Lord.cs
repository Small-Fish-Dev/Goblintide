namespace GameJam;

public partial class Lord : BaseCharacter
{
	public override float HitPoints { get; set; } = 6f;
	public override float AttackPower { get; set; } = 0.5f;
	public override float AttackSpeed { get; set; } = 0.5f;
	public override float WalkSpeed { get; set; } = 120f;

	public override FactionType Faction { get; set; } = FactionType.Goblins;

	public override float CollisionWidth { get; set; } = 20f;
	public override float CollisionHeight { get; set; } = 40f;

	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "Player" );
		Tags.Add( "Pushable" );
		Tags.Add( Faction.ToString() );
	}

	// An example BuildInput method within a player's Pawn class.
	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

		var look = Input.AnalogLook;

		var viewAngles = ViewAngles;
		viewAngles += look;
		ViewAngles = viewAngles.Normal;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		SimulateController();
		SimulateAnimations();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		SimulateController(); // Smooth push
		SimulateCamera();
		SimulateAnimations();
	}
}
