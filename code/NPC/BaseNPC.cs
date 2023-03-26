namespace GameJam;

public enum Faction
{
	None,
	Goblins,
	Humans,
	Nature
}

[Prefab, Category( "NPC" )]
public partial class BaseNPC : AnimatedEntity
{

	[Prefab, Category( "Stats" )]
	public float HitPoints { get; set; } = 6f;
	[Prefab, Category( "Stats" )]
	public float AttackPower { get; set; } = 0.5f;
	[Prefab, Category( "Stats" )]
	public float AttackSpeed { get; set; } = 0.5f;
	[Prefab, Category( "Stats" )] 
	public float WalkSpeed { get; set; } = 120f;

	[Prefab, Category( "Character" )]
	public Faction Faction { get; set; }

	[Prefab, Category( "Character" )]
	public float CollisionWidth { get; set; } = 20f;
	[Prefab, Category( "Character" )]
	public float CollisionHeight { get; set; } = 40f;
	public BBox CollisionBox => new( new Vector3( -CollisionWidth / 2f, -CollisionWidth / 2f, 0f ), new Vector3( CollisionWidth / 2f, CollisionWidth / 2f, CollisionHeight ) );

	public BaseNPC() {}

	public override void Spawn()
	{

		base.Spawn();
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "NPC" );
		Tags.Add( "Pushable" );
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


	[ConCmd.Admin("npc")]
	public static void SpawnTest( string type = "goblin", int amount = 1 )
	{
		var player = ConsoleSystem.Caller.Pawn as Lord;

		for( int i = 0; i < amount; i++ )
		{
			var guy = BaseNPC.FromPrefab( $"prefabs/npcs/{type}.prefab" );
			guy.Position = player.Position + Vector3.Random.WithZ( 0 ) * 100f;
		}
	}
}
