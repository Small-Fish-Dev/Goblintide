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

		var pushOffset = Vector3.Zero;

		foreach ( var toucher in touchingEntities )
		{
			var direction = (Position - toucher.Position).WithZ(0).Normal;
			var distance = Position.DistanceSquared( toucher.Position );
			var maxDistance = 250f;

			pushOffset = direction * Math.Max( maxDistance - distance, 0f ) * Time.Delta * 3f;
		}

		var helper = new MoveHelper( Position, Velocity + pushOffset );

		helper.Trace = helper.Trace
						.Size( CollisionBox.Mins, CollisionBox.Maxs )
						.WithoutTags( "NPC", "Player" )
						.Ignore( this );

		helper.TryMove( Time.Delta );

		Position = helper.Position;
		Velocity = helper.Velocity;

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

