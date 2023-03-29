using Sandbox.Utility;
using System.IO;

namespace GameJam;

public class Town
{
	public static Town Current { get; set; }
	public float TownSize { get; set; } = 0f;
	public Vector3 Position { get; set; } = Vector3.Zero;
	public Vector2 Bounds { get; set; } = Vector2.Zero;
	public int Seed => TownSize.GetHashCode();
	internal List<BaseEntity> townEntities = new();

	public static Dictionary<string,float> PlaceableProps { get; set; } = new()
	{
		{ "prefabs/props/barrel.prefab", 1f },
		{ "prefabs/props/largecrate.prefab", 2f },
		{ "prefabs/props/smallcrate.prefab", 10f },
	};


	public Town() { }

	public float PerlinValue( float x = 0f, float y = 0f, float scale = 10f )
	{
		return Noise.Perlin( Seed / 1000f + x / scale, Seed / 1000f + y / scale );
	}

	public static Town GenerateTown( Vector3 position, float townSize = 100f, float density = 3f )
	{
		Current?.DeleteTown();

		Current = new Town();
		Current.TownSize = townSize;
		var rand = new Random( Current.Seed );

		float townWidth = 300f * (1f + townSize / 50f);
		
		for( float x = -townWidth; x <= townWidth; x += 100f / density )
		{
			for ( float y = -townWidth; y <= townWidth; y += 100f / density )
			{
				if ( Current.PerlinValue( x, y ) <= 0.4f )
				{
					var spawnedProp = BaseProp.FromPrefab( WeightedList.RandomKey( rand, PlaceableProps ) );

					var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
					var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
					spawnedProp.Position = position + new Vector3( x + randomOffsetX, y + randomOffsetY, 0 );
					spawnedProp.Rotation = Rotation.FromYaw( rand.Next( 360 ) );
					Current.townEntities.Add( spawnedProp );
				}
			}
		}

		Vector2 minBounds = new();
		Vector2 maxBounds = new();

		foreach( var entity in Current.townEntities )
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

		Current.Bounds = maxBounds - minBounds;

		return Current;
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
