namespace GameJam;

public partial class Lord
{
	#region Rotation Configuration

	private const float RotationLerpMultiplier = 4.0f;

	#endregion

	#region Rotation Variables

	#endregion

	private void SimulateRotation()
	{
		if ( InputDirection.Length == 0 )
			return;

		var direction = Rotation.LookAt( InputDirection );

		var proposedRotation = Rotation.Slerp( Rotation, direction,
			Velocity.Length / 170 * Time.Delta * RotationLerpMultiplier );
		{
			// Remove roll from lerped rotation
			var angles = proposedRotation.Angles();
			angles.roll = 0;
			angles.pitch = 0;
			proposedRotation = angles.ToRotation();
		}

		Rotation = proposedRotation;
	}

	public void SimulateController()
	{
		var wishVelocity = InputDirection.WithZ( 0 ) * WalkSpeed * (Input.Down( InputButton.Run ) ? 1.5f : 1f);

		Velocity = Vector3.Lerp( Velocity, wishVelocity, 15f * Time.Delta )
			.WithZ( Velocity.z );

		if ( GroundEntity == null )
			Velocity -= Vector3.Down * Game.PhysicsWorld.Gravity * Time.Delta;

		if ( Input.Pressed( InputButton.Jump ) )
			if ( GroundEntity != null )
				Velocity += Vector3.Up * 200f;

		var pushOffset = Vector3.Zero;

		foreach ( var toucher in touchingEntities )
		{
			var direction = (Position - toucher.Position).WithZ( 0 ).Normal;
			var distance = Position.DistanceSquared( toucher.Position );
			var maxDistance = 250f;

			pushOffset = direction * Math.Max( maxDistance - distance, 0f ) * Time.Delta * 3f;
		}

		var helper = new MoveHelper( Position, Velocity + pushOffset ) { MaxStandableAngle = 30f };

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

		SimulateRotation();
	}

	internal List<BaseCharacter> touchingEntities = new();

	public override void StartTouch( Entity other )
	{
		base.Touch( other );

		if ( !Game.IsServer ) return;

		if ( other is BaseCharacter toucher && other.Tags.Has( "Pushable" ) )
			touchingEntities.Add( toucher );
	}


	public override void EndTouch( Entity other )
	{
		base.Touch( other );

		if ( !Game.IsServer ) return;

		if ( other is BaseCharacter toucher && touchingEntities.Contains( toucher ) )
			touchingEntities.Remove( toucher );
	}
}
