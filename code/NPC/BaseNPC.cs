namespace GameJam;

[Prefab, Category( "NPC" )]
public partial class BaseNPC : AnimatedEntity
{

	[Property, Category( "Character" )] 
	public string TypeName { get; set; } = "Base NPC";
	[Property, Category( "Character" )] 
	public float CollisionWidth { get; set; } = 20f;
	[Property, Category( "Character" )] 
	public float CollisionHeight { get; set; } = 40f;
	[Net, Property, Category( "Character" )] 
	public float WalkSpeed { get; set; } = 160f;
	public BBox CollisionBox => new( new Vector3( -CollisionWidth / 2f, -CollisionWidth / 2f, 0f ), new Vector3( CollisionWidth / 2f, CollisionWidth / 2f, CollisionHeight ) );

	public BaseNPC() {}

	public override void Spawn()
	{

		base.Spawn();
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableAllCollisions = true;
		EnableDrawing = true;

		Tags.Add( "NPC" );
	}

	public static BaseNPC FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<BaseNPC>( prefabName, out var settler ) )
		{
			return settler;
		}

		return null;
	}

	public Vector2 TimeBetweenIdleMove => new Vector2( 1f, 3f );
	internal TimeUntil nextIdleMode { get; set; } = 0f;

	[Event.Tick.Server]
	public virtual void Think()
	{
		ComputeNavigation();
		ComputeMotion();
		ComputeAnimations(); 
		
		if ( nextIdleMode )
		{
			var randomSpot = Position + Vector3.Random.WithZ(0) * 200f ;
			NavigateTo( randomSpot );
			nextIdleMode = Game.Random.Float( TimeBetweenIdleMove.x, TimeBetweenIdleMove.y );
		}
	}

}
