using GameJam.Props.Collectable;
using GameJam.Util;

namespace GameJam;

[Prefab, Category( "Prop" )]
public partial class BaseProp : BaseEntity
{

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
		Faction = DefaultFaction;
		HitPoints = MaxHitPoints;

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
			if ( result != null && this != null && this.IsValid() )
				Breakables.Break( this, result );
		}

		Sound.FromWorld( "sounds/physics/breaking/break_wood_plank.sound", Position );

		string particlePath = "particles/wood_shatter.vpcf";
		if ( GetWidth() > 100f ) particlePath = "particles/wood_shatter_large.vpcf";

		Particles.Create( particlePath, Position );

		if ( GoldDropped.x > 0 )
		{
			var gold = BaseCollectable.FromPrefab( "prefabs/collectables/gold.prefab" );
			gold.Value = Game.Random.Int( (int)GoldDropped.x, (int)GoldDropped.y );
			gold.Position = Position;
		}
		if ( WoodDropped.x > 0 )
		{
			var wood = BaseCollectable.FromPrefab( "prefabs/collectables/wood.prefab" );
			wood.Value = Game.Random.Int( (int)WoodDropped.x, (int)WoodDropped.y );
			wood.Position = Position;
		}
		if ( FoodDropped.x > 0 )
		{
			var food = BaseCollectable.FromPrefab( "prefabs/collectables/food.prefab" );
			food.Value = Game.Random.Int( (int)FoodDropped.x, (int)FoodDropped.y );
			food.Position = Position;
		}

		base.Kill();
	}

	public override void Damage( float amount, BaseCharacter attacker )
	{
		var direction = (attacker.Position - Position).Normal;
		var position = Position + GetHeight() / 2f + direction * GetWidth();
		Particles.Create( "particles/impact.wood.vpcf", position );

		base.Damage( amount, attacker );
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
