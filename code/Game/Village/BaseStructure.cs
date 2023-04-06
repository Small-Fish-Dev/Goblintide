using Sandbox.UI;

namespace GameJam;

[Prefab]
public partial class BaseStructure : ModelEntity
{
	[Prefab, Net]
	public string Title { get; set; }

	[Prefab, Net]
	public int Wood { get; set; }

	[Prefab, Net]
	public int Women { get; set; }

	public BuildingEntry Entry { get; set; }

	public override void Spawn()
	{
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		foreach ( var component in Components.GetAll<StructureComponent>() )
			component.Initialize();

		Tags.Add( "solid" );
		Tags.Add( "structure" );
	}

	public override void ClientSpawn()
	{
		foreach ( var component in Components.GetAll<StructureComponent>() )
			component.Initialize();
	}

	protected override void OnDestroy()
	{
		foreach ( var component in Components.GetAll<StructureComponent>() )
			component.OnDestroy();
	}

	public static BaseStructure FromPrefab( string prefabName )
	{
		var path = $"prefabs/structures/{prefabName}.prefab";
		if ( PrefabLibrary.TrySpawn<BaseStructure>( path, out var structure ) )
			return structure;

		return null;
	}

	[ConCmd.Server]
	public static void RequestBuild( string prefabName, Vector3 mins, Vector3 maxs )
	{
		if ( ConsoleSystem.Caller.Pawn is not Lord pawn )
			return;

		var town = GameMgr.CurrentTown;
		if ( town == null )
			return;

		// Find prefab.
		var prefab = ResourceLibrary.GetAll<Prefab>()
			.FirstOrDefault( p => p.ResourceName == prefabName );
		if ( prefab == null )
			return;

		// Make sure we are able to place it.
		var bounds = new BBox( mins, maxs );
		var boundsTrace = Trace.Box( bounds, Vector3.Zero, Vector3.Zero )
			.WithTag( "structure" )
			.Run();

		if ( boundsTrace.Entity is not null )
		{
			EventLogger.Send( To.Everyone, "<red>There is something in the way.</red>" );
			return;
		}

		var wood = prefab.Root.GetValue<int>( "Wood" );
		var women = prefab.Root.GetValue<int>( "Women" );
		if ( GameMgr.TotalWood < wood || GameMgr.TotalWomen < women )
		{
			EventLogger.Send( To.Everyone, "<red>You have insufficient resources for that.</red>" );
			return;
		}

		// Create structure.
		var entry = new BuildingEntry()
		{
			PrefabName = prefabName,
			Position = bounds.Center.WithZ( town.Position.z )
		};

		if ( !VillageState.TrySpawnStructure( entry, out var structure ) )
			return;

		// Take from resources.
		GameMgr.TotalWood -= wood;
		GameMgr.TotalWomen -= women;
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( model == null || Game.IsClient )
			return;

		var navBlocker = new NavBlockerEntity();
		navBlocker.PhysicsClear();
		navBlocker.Model = model;
		navBlocker.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		navBlocker.Position = Position;
		navBlocker.Rotation = Rotation;
		navBlocker.PhysicsEnabled = false;
		navBlocker.EnableDrawing = false;
		navBlocker.Enable();
		navBlocker.SetParent( this );
	}
}
