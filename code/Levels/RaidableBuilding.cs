using GameJam.Props.Collectable;
using GameJam.Util;

namespace GameJam;

[Prefab, Category( "Buildings" )]
public partial class RaidableBuilding : ModelEntity
{

	[Prefab, Category( "Loot" ), ResourceType( "prefab" )]
	public Dictionary<string, float> CollectableInsideAndProbability { get; set; } = new()
	{
	};
	[Prefab, Category( "Loot" ), Range( 0, 50, 1 )]
	public RangedFloat LootAmount { get; set; } = 0;

	public BaseCollectable CollectableInside { get; set; }
	public BaseProp Door { get; set; }
	public Vector3 DoorPosition { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "Solid" );
		Tags.Add( "Pushable" );

		if ( CollectableInsideAndProbability.Count() > 0 )
		{
			CollectableInside = BaseCollectable.FromPrefab( WeightedList.RandomKey( CollectableInsideAndProbability ) );
			CollectableInside.Position = Position + Vector3.Up * 5f;
			CollectableInside.SetParent( this );
			CollectableInside.PhysicsEnabled = false;
			CollectableInside.Locked = true;
			CollectableInside.Value = CollectableInside.Type == Collectable.Woman ? 1 : Game.Random.Int( (int)LootAmount.x, (int)LootAmount.y );
		}


		if ( Children.Count > 0 )
			if ( Children.First() is BaseProp prop )
			{
				Door = prop;
				DoorPosition = Door.Position;
			}
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		var navblocker = new NavBlockerEntity();
		navblocker.PhysicsClear();
		navblocker.Model = model;
		navblocker.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		navblocker.Position = Position;
		navblocker.Rotation = Rotation;
		navblocker.PhysicsEnabled = false;
		navblocker.EnableDrawing = false;
		navblocker.Enable();
		navblocker.SetParent( this );
	}

	[Event.Tick.Server]
	public void ReleaseLoot()
	{
		if ( Door.IsValid() && CollectableInside.IsValid() )
		{
			CollectableInside.Position = Position;
			CollectableInside.SetParent( this );
		}
		if ( CollectableInside.IsValid() && CollectableInside.Type == Collectable.Woman ) // Idk why they just fly away otherwise??????
		{
			CollectableInside.Position = Position;
			CollectableInside.SetParent( this );
		}
		if ( !Door.IsValid() && CollectableInside.IsValid() && CollectableInside.Locked ) //TODO SPAWN FIRE
		{
			CollectableInside.SetParent( null );
			CollectableInside.Locked = false;
			CollectableInside.PhysicsEnabled = true;
			CollectableInside.Position = Transform.PointToWorld( DoorPosition ) + Vector3.Up * 20f;
		}
	}

	public static RaidableBuilding FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<RaidableBuilding>( prefabName, out var house ) )
		{
			return house;
		}

		return null;
	}
}
