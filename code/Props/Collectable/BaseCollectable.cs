using GameJam.Util;

namespace GameJam.Props.Collectable;

public enum Collectable
{
	[Icon( "error" )]
	Invalid,
	[Icon( "forest" )]
	Wood,
	[Icon( "restaurant" )]
	Food,
	[Icon( "toll" )]
	Gold,
	[Icon( "woman" )]
	Woman
}

[Prefab, Category( "Collectable" )]
public partial class BaseCollectable : ModelEntity
{
	[Prefab, Category( "Collectable Type" ), Net]
	public Collectable Type { get; private set; } = Collectable.Invalid;
	[Prefab]
	public bool IsRagdoll { get; private set; } = false;
	public int Value { get; private set; } = 1;

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;
		if ( IsRagdoll )
		{
			UsePhysicsCollision = true;
			EnableSelfCollisions = true;
		}
	}

	public static BaseCollectable FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<BaseCollectable>( prefabName, out var prop ) )
		{
			return prop;
		}

		return null;
	}


	[ConCmd.Admin( "collectable" )]
	public static void SpawnTest( string type = "wood", int amount = 1 )
	{
		var player = ConsoleSystem.Caller.Pawn as Lord;

		for ( int i = 0; i < amount; i++ )
		{
			var collectable = BaseCollectable.FromPrefab( $"prefabs/collectables/{type}.prefab" );
			collectable.Position = player.Position + Vector3.Random.WithZ( 0 ) * 100f;
		}
	}
}
