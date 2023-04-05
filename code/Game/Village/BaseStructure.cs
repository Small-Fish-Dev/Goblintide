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

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( model == null )
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
