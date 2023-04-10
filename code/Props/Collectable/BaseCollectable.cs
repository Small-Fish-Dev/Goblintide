using GoblinGame.Util;

namespace GoblinGame.Props.Collectable;

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
public partial class BaseCollectable : BaseEntity
{
	[Prefab, Category( "Collectable Type" ), Net]
	public Collectable Type { get; private set; } = Collectable.Invalid;
	[Prefab]
	public bool IsRagdoll { get; private set; } = false;
	public BaseCharacter StolenBy { get; set; } = null;
	public int Value { get; set; } = 1;
	public bool Locked { get; set; } = false;
	public override bool BlockNav => false;

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
			Town.TownEntities.Add( prop );
			return prop;
		}

		return null;
	}
}
