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
	public int AttackedBy { get; set; } = 0;

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
}
