namespace GameJam;

public class Town
{
	public static Town Current { get; set; }
	public bool Generated { get; private set; } = false;
	public float TownSize { get; set; } = 0f;
	public Vector3 Position { get; set; } = Vector3.Zero;
	public Vector2 Bounds { get; set; } = Vector2.Zero;
	public int Seed => TownSize.GetHashCode();
	internal List<Entity> townEntities = new();

	public static Dictionary<string, float> PlaceableHouses { get; set; } = new()
	{
		{ "models/houses/house_a.vmdl", 1f },
	};

	public static Dictionary<string, float> PlaceableBigProps { get; set; } = new()
	{
		{ "prefabs/props/stand.prefab", 0.3f },
		{ "prefabs/props/waggon.prefab", 0.5f },
	};

	public static Dictionary<string, float> PlaceableSmallProps { get; set; } = new()
	{
		{ "prefabs/props/barrel.prefab", 2f },
		{ "prefabs/props/largecrate.prefab", 2f },
		{ "prefabs/props/smallcrate.prefab", 3f },
	};

	public static Dictionary<string, float> PlaceablePeople { get; set; } = new()
	{
		{ "prefabs/npcs/soldier.prefab", 1f },
		{ "prefabs/npcs/villager.prefab", 6f },
	};

	public Town() { }

	public float NoiseValue( float x = 0f, float y = 0f, float scale = 10f )
	{
		return Noise.Fbm( 2, Seed / 100f + x / scale, Seed / 100f + y / scale );
	}

	internal async Task<bool> PlaceHouses( Random rand, Vector3 position, float x, float y, float density, Vector2 threshold )
	{
		var noise = NoiseValue( x, y );
		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var spawnedHouse = new BaseEntity();
			spawnedHouse.SetModel( WeightedList.RandomKey( rand, PlaceableHouses ) );
			spawnedHouse.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			await GameTask.RunInThreadAsync( () =>
			{
				var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
				var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
				spawnedHouse.Position = position + new Vector3( x + randomOffsetX, y + randomOffsetY, 0 );
				spawnedHouse.Rotation = Rotation.LookAt( spawnedHouse.Position - position);
				var traceCheck = Trace.Body( spawnedHouse.PhysicsBody, spawnedHouse.Position )
				.Ignore( spawnedHouse )
				.EntitiesOnly()
				.Run();

				if ( traceCheck.Hit )
				{
					spawnedHouse.Delete();
					return false;
				}
				else
				{
					Current.townEntities.Add( spawnedHouse );
					return true;
				}
			} );
		}

		return false;
	}

	internal async Task<bool> TryForBigProp( Random rand, Vector3 position, float x, float y, float density, Vector2 threshold )
	{
		var noise = NoiseValue( x, y );
		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var spawnedProp = BaseProp.FromPrefab( WeightedList.RandomKey( rand, PlaceableBigProps ) );

			await GameTask.RunInThreadAsync( () =>
			{
				var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
				var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
				spawnedProp.Position = position + new Vector3( x + randomOffsetX, y + randomOffsetY, 0 );
				spawnedProp.Rotation = Rotation.FromYaw( rand.Next( 360 ) );

				var traceCheck = Trace.Body( spawnedProp.PhysicsBody, spawnedProp.Position )
				.Ignore( spawnedProp )
				.EntitiesOnly()
				.Run();

				if ( traceCheck.Hit )
				{
					spawnedProp.Delete();
					return false;
				}
				else
				{
					Current.townEntities.Add( spawnedProp );
					return true;
				}
			} );
		}

		return false;
	}

	internal async Task<bool> TryForSmallProp( Random rand, Vector3 position, float x, float y, float density, Vector2 threshold )
	{
		var noise = NoiseValue( x, y );
		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var spawnedProp = BaseProp.FromPrefab( WeightedList.RandomKey( rand, PlaceableSmallProps ) );

			await GameTask.RunInThreadAsync( () =>
			{
				var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
				var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
				spawnedProp.Position = position + new Vector3( x + randomOffsetX, y + randomOffsetY, 0 );
				spawnedProp.Rotation = Rotation.FromYaw( rand.Next( 360 ) );

				var traceCheck = Trace.Body( spawnedProp.PhysicsBody, spawnedProp.Position )
				.Ignore( spawnedProp )
				.EntitiesOnly()
				.Run();

				if ( traceCheck.Hit )
				{
					spawnedProp.Delete();
					return false;
				}
				else
				{
					Current.townEntities.Add( spawnedProp );
					return true;
				}
			} );
		}

		return false;
	}

	internal async Task<bool> TryForNPCs( Random rand, Vector3 position, float x, float y, float density, Vector2 threshold )
	{
		var noise = NoiseValue( x, y );
		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var spawnedNPC = BaseNPC.FromPrefab( WeightedList.RandomKey( rand, PlaceablePeople ) );

			await GameTask.RunInThreadAsync( () =>
			{
				var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
				var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
				spawnedNPC.Position = position + new Vector3( x + randomOffsetX, y + randomOffsetY, 0 );
				spawnedNPC.Rotation = Rotation.FromYaw( rand.Next( 360 ) );

				Current.townEntities.Add( spawnedNPC );
			} );

			return true;
		}

		return false;
	}

	public static async void GenerateTown( Vector3 position, float townSize, float density )
	{
		Current?.DeleteTown();

		Current = new Town();
		Current.TownSize = townSize;

		var rand = new Random( Current.Seed );
		var townWidth = 300f * (float)Math.Sqrt( townSize / 5 );
		var townWidthSquared = townWidth * townWidth;
		var mainRoadSize = 30f + townWidth / 12f;

		for ( float x = -townWidth; x <= townWidth; x += 100f / density )
		{
			for ( float y = -townWidth; y <= townWidth; y += 100f / density )
			{
				var squaredDistance = x * x + y * y;

				if ( squaredDistance > townWidthSquared ) continue;
				if ( y < mainRoadSize && y > -mainRoadSize) continue;

				if ( await Current.PlaceHouses( rand, position, x, y, density, new Vector2( 0f, 0.35f ) ) )
				continue;
			}
		}

		for ( float x = -townWidth; x <= townWidth; x += 100f / density )
		{
			for ( float y = -townWidth; y <= townWidth; y += 100f / density )
			{
				var squaredDistance = x * x + y * y;

				if ( squaredDistance > townWidthSquared ) continue;
				if ( y < mainRoadSize && y > -mainRoadSize ) continue;

				if ( await Current.TryForBigProp( rand, position, x, y, density, new Vector2( 0.35f, 0.4f ) ) )
					continue;
			}
		}

		for ( float x = -townWidth; x <= townWidth; x += 100f / density )
		{
			for ( float y = -townWidth; y <= townWidth; y += 100f / density )
			{
				var squaredDistance = x * x + y * y;

				if ( squaredDistance > townWidthSquared ) continue;
				if ( y < mainRoadSize && y > -mainRoadSize ) continue;

				if ( await Current.TryForSmallProp( rand, position, x, y, density * 2, new Vector2( 0.35f, 0.45f ) ) )
					continue;

				if ( await Current.TryForNPCs( rand, position, x, y, density, new Vector2( 0.8f, 1f ) ) )
					continue;
			}
		}

		var minBounds = new Vector2();
		var maxBounds = new Vector2();

		foreach ( var entity in Current.townEntities )
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
		Current.Generated = true;
	}

	public void DeleteTown()
	{
		foreach ( var entity in townEntities )
			entity.Delete();
	}

	[ConCmd.Admin( "town" )]
	public static void CreateTown( float townSize = 100f, float density = 2f )
	{
		if ( ConsoleSystem.Caller.Pawn is not Lord player )
			return;

		Town.GenerateTown( player.Position, townSize, density );
	}
}
