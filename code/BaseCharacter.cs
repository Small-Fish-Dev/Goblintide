namespace GameJam;

public partial class BaseCharacter : BaseEntity
{

	public virtual float CollisionWidth { get; set; } = 20f;
	public virtual float CollisionHeight { get; set; } = 40f;
	public virtual BBox CollisionBox => new( new Vector3( -CollisionWidth / 2f, -CollisionWidth / 2f, 0f ), new Vector3( CollisionWidth / 2f, CollisionWidth / 2f, CollisionHeight ) );

	public override float GetWidth() => CollisionWidth;
	public override float GetHeight() => CollisionHeight;

	public override bool BlockNav { get; set; } = false;

	public BaseCharacter() {}

	public override void Spawn()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "Pushable" );
		Tags.Add( Faction.ToString() );
	}
}
