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

		Velocity = Vector3.Lerp( Velocity, WishVelocity, 15f * Time.Delta )
			.WithZ( Velocity.z );

		if ( GroundEntity == null )
			Velocity -= Vector3.Down * Game.PhysicsWorld.Gravity * Time.Delta;

		var helper = new MoveHelper( Position, Velocity ) { MaxStandableAngle = MaxWalkableAngle };

		helper.Trace = helper.Trace
						.Size( CollisionBox.Mins, CollisionBox.Maxs )
						.WithoutTags( "NPC" )
						.Ignore( this );

		helper.TryUnstuck();
		helper.TryMoveWithStep( Time.Delta, StepSize );

		Position = helper.Position;
		Velocity = helper.Velocity;

		if ( Velocity.z <= StepSize )
		{
			var tr = helper.TraceDirection( Vector3.Down );

			GroundEntity = tr.Entity;

			if ( GroundEntity != null )
			{
				Position += tr.Distance * Vector3.Down;

				if ( Velocity.z < 0.0f )
					Velocity = Velocity.WithZ( 0 );
			}
		}
		else
			GroundEntity = null;
	}
}

