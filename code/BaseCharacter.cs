namespace GameJam;

public enum FactionType
{
	None,
	Goblins,
	Humans,
	Nature
}

public partial class BaseCharacter : AnimatedEntity
{

	public virtual float HitPoints { get; set; } = 6f;
	public virtual FactionType Faction { get; set; }
	public int TotalAttackers { get; set; } = 0;
	public BaseCharacter LastAttackedBy { get; set; } = null;
	public TimeSince LastAttacked { get; set; } = 0f;

	public virtual float CollisionWidth { get; set; } = 20f;
	public virtual float CollisionHeight { get; set; } = 40f;
	public virtual BBox CollisionBox => new( new Vector3( -CollisionWidth / 2f, -CollisionWidth / 2f, 0f ), new Vector3( CollisionWidth / 2f, CollisionWidth / 2f, CollisionHeight ) );


	public BaseCharacter() {}

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "Pushable" );
		Tags.Add( Faction.ToString() );
	}

	public virtual void Damage( float amount, BaseCharacter attacker )
	{
		HitPoints = Math.Max( HitPoints - amount, 0 );
		LastAttacked = 0f;
		LastAttackedBy = attacker;

		if ( HitPoints <= 0 )
		{
			Kill();
		}
	}

	public virtual void Kill()
	{
		Delete();
	}
}
