using Sandbox.UI;

namespace GameJam;

[Prefab, Category( "Structures" )]
public partial class BaseStructure : ModelEntity
{
	[Prefab, Net]
	public string Title { get; set; }

	[Prefab, Net]
	public int Wood { get; set; }

	[Prefab, Net]
	public int Women { get; set; }

	[Prefab, Net]
	public int Food { get; set; }

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
		RenderColor = Color.White.WithAlpha( 0 );

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

	[Event.Tick]
	private void tick()
	{
		if ( Game.IsServer )
			return;

		if ( RenderColor == Color.White )
			return;

		RenderColor = RenderColor.WithAlpha( RenderColor.a + Time.Delta );
		if ( RenderColor.a >= 1 )
			RenderColor = Color.White;

		if ( SceneObject != null )
			DebugOverlay.Box( SceneObject.Bounds.Mins, SceneObject.Bounds.Maxs, RenderColor.WithAlpha( 1 - RenderColor.a ) );
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
			EventLogger.Send( To.Everyone, "<red>There is something in the way.</red>", 3 );
			return;
		}

		var wood = prefab.Root.GetValue<int>( "Wood" );
		var women = prefab.Root.GetValue<int>( "Women" );
		var food = prefab.Root.GetValue<int>( "Food" );
		if ( GameMgr.TotalWood < wood || GameMgr.TotalWomen < women || GameMgr.TotalFood < food )
		{
			EventLogger.Send( To.Everyone, "<red>You have insufficient resources for that.</red>", 3 );
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

		EventLogger.Send( To.Everyone, $"Building a <lightblue>{prefab.Root.GetValue<string>( "Title" ).ToUpper()}</>.", 8 );

		// Take from resources.
		GameMgr.TotalWood -= wood;
		GameMgr.TotalWomen -= women;
		GameMgr.TotalFood -= food;
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
