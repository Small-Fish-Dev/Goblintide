namespace GameJam;

public partial class Lord : AnimatedEntity
{

	public BBox CollisionBox => new BBox( new Vector3( -12f, -12f, 0f ), new Vector3( 12f, 12f, 48f ) );

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableDrawing = true;
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

		SimulateCamera();
		SimulateAnimations();
	}
}
