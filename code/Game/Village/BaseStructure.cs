﻿namespace GameJam;

[Prefab]
public partial class BaseStructure : ModelEntity
{
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
}