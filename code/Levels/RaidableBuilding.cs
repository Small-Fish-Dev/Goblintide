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
			CollectableInside.EnableAllCollisions = false;
			CollectableInside.UsePhysicsCollision = false;
			CollectableInside.EnableSelfCollisions = false;
			CollectableInside.SetParent( this );
			CollectableInside.Locked = true;
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
		if ( !Door.IsValid() && CollectableInside.IsValid() && CollectableInside.Locked )
		{
			CollectableInside.SetParent( null );
			CollectableInside.Locked = false;
			CollectableInside.EnableAllCollisions = true;
			CollectableInside.UsePhysicsCollision = true;
			CollectableInside.EnableSelfCollisions = true;
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
