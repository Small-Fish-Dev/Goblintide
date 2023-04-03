namespace GameJam;

public enum ItemType
{
	None,
	Armor,
	Weapon
}

[Prefab, Category("Items")]
public partial class BaseItem : BaseEntity
{

	[Prefab, Category( "Stats" )]
	public float IncreasedAttack { get; set; } = 0f;
	[Prefab, Category( "Stats" )]
	public float IncreasedHealth { get; set; } = 0f;
	[Prefab, Category( "Stats" )]
	public ItemType Type { get; set; } = ItemType.None;
	public bool Equipped = false;

	public override float GetWidth() => 20f;
	public override float GetHeight() => 10f;

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		EnableAllCollisions = true;
		PhysicsEnabled = true;
		UsePhysicsCollision = true;
	}
	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );
	}

	public static BaseItem FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<BaseItem>( prefabName, out var item ) )
		{
			GameMgr.CurrentTown?.TownEntities.Add( item );
			return item;
		}

		return null;
	}
}
