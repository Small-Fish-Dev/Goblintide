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
	[Icon( "star" )]
	Special
}

[Prefab, Category( "Collectable" )]
public partial class BaseCollectable : ModelEntity
{
	[Prefab, Category( "Collectable Type" ), Net]
	public Collectable Type { get; private set; } = Collectable.Invalid;
	[Prefab, Category( "Value" ), Range( 1, 5, step: 1 )]
	public int Value { get; private set; } = 1;
	[Prefab, Category( "Collision Box Size" ), Range( 10, 50 )]
	public float CollisionBoxSize { get; private set; } = 10.0f;

	public float RotationSpeed => 10.0f;
	public float FloatPeriod => 0.5f;
	public float FloatHeight => 10.0f;

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromAABB( PhysicsMotionType.Static, new( -CollisionBoxSize, -CollisionBoxSize, 0 ), new( CollisionBoxSize, CollisionBoxSize, CollisionBoxSize * 2 ) );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "Trigger" );
	}

	[Event.Client.Frame]
	public void Tick()
	{
		SceneObject.Rotation *= Rotation.FromYaw( Time.Delta * RotationSpeed );
		SceneObject.Position = Position + Vector3.Up * FloatHeight * (1 + MathF.Sin( Time.Now * FloatPeriod ));
	}

	public override void Touch( Entity other )
	{
		base.Touch( other );

		Log.Info( $"Gotta pick up that {Type}!" );
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
