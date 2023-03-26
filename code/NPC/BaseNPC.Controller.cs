namespace GameJam;

public partial class BaseNPC
{
	[Net] public float WishSpeed { get; private set; } = 0f;
	[Net] public Vector3 Direction { get; set; } = Vector3.Zero;
	public Vector3 WishVelocity => Direction.Normal * WishSpeed;
	public Rotation WishRotation => Rotation.LookAt( Direction, Vector3.Up );
	public float StepSize => 16f;
	public float MaxWalkableAngle => 60f;

	public virtual void ComputeMotion()
	{
		if ( Direction != Vector3.Zero )
			WishSpeed = WalkSpeed;
		else
			WishSpeed = 0f;

		Velocity = Vector3.Lerp( Velocity, WishVelocity, Time.Delta * 5f )
			.WithZ( Velocity.z );

		var helper = new MoveHelper( Position, Velocity );

		helper.Trace = helper.Trace
						.Size( CollisionBox.Mins, CollisionBox.Maxs )
						.WithoutTags( "NPC" )
						.Ignore( this );

		helper.TryMove( Time.Delta );

		Position = helper.Position;
		Velocity = helper.Velocity;

	}
}

