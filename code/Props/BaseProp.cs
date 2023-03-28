namespace GameJam;

[Prefab, Category( "Prop" )]
public partial class BaseProp : BaseEntity
{

	[Prefab, Category( "Stats" )]
	public override float HitPoints { get; set; } = 0.5f;

	[Prefab, Category( "Stats" )]
	public virtual RangedFloat GoldDropped { get; set; } = new RangedFloat( 0f, 0f );
	[Prefab, Category( "Stats" )]
	public virtual RangedFloat WoodDropped { get; set; } = new RangedFloat( 0f, 0f );
	[Prefab, Category( "Stats" )]
	public virtual RangedFloat FoodDropped { get; set; } = new RangedFloat( 0f, 0f );
	[Prefab, Category( "Visual" )]
	public virtual bool IsBreakable { get; set; } = true;

	public BaseProp() { }

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "Solid" );
		Tags.Add( Faction.ToString() );
	}

	public override void Kill()
	{

		if ( IsBreakable )
		{
			var result = new Breakables.Result();
			Breakables.Break( this, result );
		}

		base.Kill();
	}

	public static BaseProp FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<BaseProp>( prefabName, out var prop ) )
		{
			return prop;
		}

		return null;
	}
}
