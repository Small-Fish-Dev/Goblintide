using System.IO;

namespace GameJam;

public class Town
{
	public float TownSize { get; set; } = 0f;
	public Vector3 Position { get; set; } = Vector3.Zero;
	public Vector2 Bounds { get; set; } = Vector2.Zero;
	public int Seed => TownSize.GetHashCode();
	internal int currentGenerationSeed { get; set; } = 0;
	public Random RandomFromSeed
	{
		get
		{
			currentGenerationSeed += 200;
			return new Random( Seed + currentGenerationSeed );
		}
	}
	internal List<BaseEntity> townEntities = new();

	public static List<string> PlaceableProps { get; set; } = new()
	{
		"prefabs/props/barrel.prefab",
		"prefabs/props/largecrate.prefab",
		"prefabs/props/smallcrate.prefab"
	};

	public Town() { }

	public static Town GenerateTown( Vector3 position, float townSize = 100f, float density = 1f )
	{
		var generatedTown = new Town();
		generatedTown.TownSize = townSize;
		float townWidth = 300f * (1f + townSize / 50f);
		
		for( float x = -townWidth; x <= townWidth; x += 100f / density )
		{
			for ( float y = -townWidth; y <= townWidth; y += 100f / density )
			{
				if ( generatedTown.RandomFromSeed.Next( 10 ) < 2 )
				{
					var randomPropId = generatedTown.RandomFromSeed.Next( PlaceableProps.Count - 1 );
					var randomProp = PlaceableProps[randomPropId];
					var spawnedProp = BaseProp.FromPrefab( randomProp );

					var randomOffsetX = generatedTown.RandomFromSeed.Next( -(int)(50f / density), (int)(50f / density) );
					var randomOffsetY = generatedTown.RandomFromSeed.Next( -(int)(50f / density), (int)(50f / density) );
					spawnedProp.Position = position + new Vector3( x + randomOffsetX, y + randomOffsetY, 0 );
					spawnedProp.Rotation = Rotation.FromYaw( generatedTown.RandomFromSeed.Next( 360 ) );
					generatedTown.townEntities.Add( spawnedProp );
				}
			}
		}

		Vector2 minBounds = new();
		Vector2 maxBounds = new();

		foreach( var entity in generatedTown.townEntities )
		{
			if ( entity.Position.x - position.x < minBounds.x )
				minBounds.x = entity.Position.x;
			if ( entity.Position.y - position.y < minBounds.y )
				minBounds.y = entity.Position.y;
			if ( entity.Position.x - position.x > maxBounds.x )
				maxBounds.x = entity.Position.x;
			if ( entity.Position.y - position.y > maxBounds.y )
				maxBounds.y = entity.Position.y;
		}

		generatedTown.Bounds = maxBounds - minBounds;

		return generatedTown;
	}

	public void DeleteTown()
	{
		foreach( var entity in townEntities )
		{
			entity.Delete();
		}
	}


	[ConCmd.Admin( "town" )]
	public static void CreateTown( float townSize = 100f, float density = 1f )
	{
		var player = ConsoleSystem.Caller.Pawn as Lord;

		Town.GenerateTown( player.Position, townSize, density );
	}
}
