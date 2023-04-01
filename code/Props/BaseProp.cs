using GameJam.Util;

namespace GameJam;

[Prefab, Category( "Prop" )]
public partial class BaseProp : BaseEntity
{

	[Prefab, Category( "Stats" )]
	public override float HitPoints { get; set; } = 0.5f;

	[Prefab, Category( "Stats" ), Range( 0, 10, 1 )]
	public virtual RangedFloat GoldDropped { get; set; } = 0;
	[Prefab, Category( "Stats" ), Range( 0, 10, 1 )]
	public virtual RangedFloat WoodDropped { get; set; } = 0;
	[Prefab, Category( "Stats" ), Range(0, 10, 1)]
	public virtual RangedFloat FoodDropped { get; set; } = 0;
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
		Tags.Add( "Pushable" );
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


	[ConCmd.Admin( "prop" )]
	public static void SpawnTest( string type = "barrel", int amount = 1 )
	{
		var player = ConsoleSystem.Caller.Pawn as Lord;

		for ( int i = 0; i < amount; i++ )
		{
			var prop = BaseProp.FromPrefab( $"prefabs/props/{type}.prefab" );
			prop.Position = player.Position + Vector3.Random.WithZ( 0 ) * 100f;
		}
	}
}
