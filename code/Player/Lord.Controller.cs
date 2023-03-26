namespace GameJam;

public partial class Lord
{
	[Net] public float WalkSpeed { get; set; } = 120f;
	[Net] public float RunSpeed { get; set; } = 200f;

	public void SimulateController()
	{
		Rotation = Rotation.FromYaw( ViewAngles.yaw );

		var wishVelocity = Rotation.FromYaw( Rotation.Yaw() ) * InputDirection.WithZ(0) * ( Input.Down( InputButton.Run ) ? RunSpeed : WalkSpeed );

		Velocity = Vector3.Lerp( Velocity, wishVelocity, 15f * Time.Delta )
			.WithZ( Velocity.z );

		if ( GroundEntity == null )
			Velocity -= Vector3.Down * Game.PhysicsWorld.Gravity * Time.Delta;

		if ( Input.Pressed( InputButton.Jump ) )
			if ( GroundEntity != null )
				Velocity += Vector3.Up * 200f;

		var helper = new MoveHelper( Position, Velocity ) { MaxStandableAngle = 30f };

		helper.Trace = helper.Trace
						.Size( CollisionBox.Mins, CollisionBox.Maxs )
						.Ignore( this );

		helper.TryUnstuck();
		helper.TryMoveWithStep( Time.Delta, 16f );

		Position = helper.Position;
		Velocity = helper.Velocity;

		if ( Velocity.z <= 16f )
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
