namespace GameJam;

public partial class Lord : AnimatedEntity
{

	public float CollisionWidth { get; set; } = 20f;
	public float CollisionHeight { get; set; } = 40f;
	public BBox CollisionBox => new( new Vector3( -CollisionWidth / 2f, -CollisionWidth / 2f, 0f ), new Vector3( CollisionWidth / 2f, CollisionWidth / 2f, CollisionHeight ) );

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		Tags.Add( "Player" );
		Tags.Add( "Pushable" );

		EnableDrawing = true;
		EnableTouch = true;
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
