namespace GoblinGame;

public partial class Lord
{
	[Net] public bool BlockMovement { get; set; }

	#region Rotation Configuration

	private const float RotationLerp = 10.0f;
	private const float RotationPointingLerp = 20.0f;

	#endregion

	private void SimulateRotation()
	{
		var targetRotation = Pointing
			? LookDirection
			: (InputDirection.Length != 0)
				? Rotation.LookAt( InputDirection )
				: Rotation;

		var delta = Pointing 
			? RotationPointingLerp
			: RotationLerp;
		Rotation = Rotation.FromYaw( Rotation.Slerp( Rotation, targetRotation, delta * Time.Delta ).Yaw() );
	}

	public void SimulateController()
	{
		var wishVelocity = BlockMovement ? 0
			: InputDirection.WithZ( 0 ).Normal
				* WalkSpeed
				* (Input.Down( InputButton.Run )
					? 1.5f
					: 1f);

		Velocity = Vector3.Lerp( Velocity, wishVelocity, 15f * Time.Delta )
			.WithZ( Velocity.z );

		if ( GroundEntity == null )
			Velocity -= Vector3.Down * Game.PhysicsWorld.Gravity * Time.Delta;

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
		helper.TryMoveWithStep( Time.Delta, 2f );

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

		if ( !BlockMovement )
			SimulateRotation();
	}

	internal List<BaseEntity> touchingEntities = new();

	public override void StartTouch( Entity other )
	{
		base.Touch( other );

		if ( !Game.IsServer )
			return;

		if ( other is BaseEntity toucher && other.Tags.Has( "Pushable" ) )
			touchingEntities.Add( toucher );
	}


	public override void EndTouch( Entity other )
	{
		base.Touch( other );

		if ( !Game.IsServer )
			return;

		if ( other is BaseEntity toucher && touchingEntities.Contains( toucher ) )
			touchingEntities.Remove( toucher );
	}
}
