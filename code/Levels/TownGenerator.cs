using System;
using System.Collections.Generic;
using static Sandbox.CitizenAnimationHelper;

namespace GameJam;

public partial class Town
{
	public static Town Current { get; set; }
	public bool Generated { get; private set; } = false;
	public float TownSize { get; set; } = 0f;
	public Vector3 Position { get; set; } = Vector3.Zero;
	public Vector2 Bounds { get; set; } = Vector2.Zero;
	public int Seed => TownSize.GetHashCode();
	internal List<Entity> townEntities = new();
	public static List<SceneObject> TownTrees = new();

	public static Dictionary<string, float> PlaceableHouses { get; set; } = new()
	{
		{ "prefabs/props/house_a.prefab_c", 1f },
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

	public static Dictionary<string, float> PlaceableFences { get; set; } = new()
	{
		{ "prefabs/props/logwall.prefab", 130f },
		{ "prefabs/props/fence.prefab", 100f },
	};

	public static Dictionary<string, float> PlaceableTrees { get; set; } = new()
	{
		{ "models/placeholders/placeholder_tree.vmdl", 1f },
	};

	public Town() { }

	public float NoiseValue( float x = 0f, float y = 0f, float scale = 10f )
	{
		return Noise.Fbm( 2, Seed / 100f + x / scale, Seed / 100f + y / scale );
	}
	public static float NoiseFBM( float x = 0f, float y = 0f, float scale = 10f )
	{
		return Noise.Fbm( 2, x / scale, y / scale );
	}

	internal async Task<bool> TryPlaceProp( Dictionary<string, float> list, Random rand, Vector3 position, float x, float y, float density, Vector2 threshold, bool lookAtCenter = false  )
	{
		var noise = NoiseValue( x, y );
		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var spawnedEntity = BaseProp.FromPrefab( WeightedList.RandomKey( rand, list ) );

			await GameTask.RunInThreadAsync( () =>
			{
				var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
				var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * (50f / density);
				spawnedEntity.Position = position + new Vector3( x + randomOffsetX, y + randomOffsetY, 0 );
				spawnedEntity.Rotation = lookAtCenter ? Rotation.LookAt( spawnedEntity.Position - position ) : Rotation.FromYaw( rand.Next( 360 ) );
				var traceCheck = Trace.Body( spawnedEntity.PhysicsBody, spawnedEntity.Position )
				.Ignore( spawnedEntity )
				.EntitiesOnly()
				.Run();

				if ( traceCheck.Hit )
				{
					spawnedEntity.Delete();
					return false;
				}
				else
				{
					Current.townEntities.Add( spawnedEntity );
					return true;
				}
			} );
		}

		return false;
	}

	internal static async Task<bool> TryPlaceTree( Dictionary<string, float> list, Vector3 position, float x, float y, Vector2 threshold )
	{
		var noise = NoiseFBM( x, y, 3f );
		Log.Info( noise );
		if ( noise >= threshold.x && noise <= threshold.y )
		{

			await GameTask.RunInThreadAsync( () =>
			{
				var transform = new Transform( position + new Vector3( x, y, 0 ), Rotation.FromYaw( Game.Random.Int( 360 ) ), Game.Random.Float( 0.8f, 1.2f ) );
				var spawnedTree = new SceneObject( Game.SceneWorld, WeightedList.RandomKey( list ), transform );
				
				TownTrees.Add( spawnedTree );
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

	internal async Task<bool> PlaceProps( Dictionary<string, float> list, Random rand, Vector3 position, float density, Vector2 threshold, bool lookAtCenter = false )
	{

		var townWidth = 300f * (float)Math.Sqrt( TownSize / 5 );
		var townWidthSquared = townWidth * townWidth;
		var mainRoadSize = 60f + townWidth / 15f;

		for ( float x = -townWidth; x <= townWidth; x += 100f / density )
		{
			for ( float y = -townWidth; y <= townWidth; y += 100f / density )
			{
				var squaredDistance = x * x + y * y;

				if ( squaredDistance > townWidthSquared ) continue;
				if ( y < mainRoadSize && y > -mainRoadSize ) continue;

				if ( await Current.TryPlaceProp( list, rand, position, x, y, density, new Vector2( threshold.x, threshold.y ), lookAtCenter ) )
					continue;
			}
		}
		return true;
	}

	internal async Task<bool> PlaceNPCs( Dictionary<string, float> list, Random rand, Vector3 position, float density, Vector2 threshold )
	{
		var townWidth = 300f * (float)Math.Sqrt( TownSize / 5 );
		var townWidthSquared = townWidth * townWidth;
		var mainRoadSize = 60f + townWidth / 15f;

		for ( float x = -townWidth; x <= townWidth; x += 100f / density )
		{
			for ( float y = -townWidth; y <= townWidth; y += 100f / density )
			{
				var squaredDistance = x * x + y * y;

				if ( squaredDistance > townWidthSquared ) continue;
				if ( y < mainRoadSize && y > -mainRoadSize ) continue;

				if ( await Current.TryForNPCs( rand, position, x, y, density, new Vector2( threshold.x, threshold.y ) ) )
					continue;
			}
		}
		return true;
	}

	public static async Task<bool> PlaceTrees( Vector3 position, float townWidth )
	{
		foreach( var tree in TownTrees )
		{
			tree.Delete();
		}

		var clearingDistance = townWidth + 400f;
		var forestSize = clearingDistance + 1200f;
		var clearingSquared = clearingDistance * clearingDistance;
		var forestSizeSquared = forestSize * forestSize;

		for ( float x = -forestSize; x <= forestSize; x += 40f )
		{
			for ( float y = -forestSize; y <= forestSize; y += 40f )
			{
				var squaredDistance = x * x + y * y;

				if ( squaredDistance < clearingSquared ) continue;
				if ( squaredDistance > forestSizeSquared ) continue;

				if ( await TryPlaceTree( PlaceableTrees, position, x, y, new Vector2( 0f, 0.4f ) ) )
					continue;
			}
		}
		return true;
	}

	internal async Task<bool> PlaceWall( Vector3 position )
	{
		var townWidth = 300f * (float)Math.Sqrt( TownSize / 5 );
		var townDiameter = townWidth * 2 + 400f;
		var perimeter = 2 * townDiameter * Math.PI;
		var bestFence = townWidth >= 650f ? PlaceableFences.First() : PlaceableFences.Last();
		var fenceSize = bestFence.Value;
		int fenceCount = (int)Math.Ceiling( perimeter / fenceSize / 2 );
		var mainRoadSize = 60f + townWidth / 15f;

		for ( int i = 0; i < fenceCount; i++ )
		{
			var angle = i * fenceSize / (townDiameter / 2);
			var x = townDiameter / 2 * (float)Math.Cos( angle );
			var y = townDiameter / 2 * (float)Math.Sin( angle );
			if ( y < mainRoadSize && y > -mainRoadSize ) continue;

			var spawnedFence = BaseProp.FromPrefab( bestFence.Key );

			await GameTask.RunInThreadAsync( () =>
			{
				spawnedFence.Position = position + Vector3.Forward * x + Vector3.Right * y;
				spawnedFence.Rotation = Rotation.LookAt( spawnedFence.Position - position );
			});

			Current.townEntities.Add( spawnedFence );
		}

		return true;
	}

	public static async void GenerateTown( Vector3 position, float townSize, float density )
	{
		Current?.DeleteTown();

		Current = new Town();
		Current.TownSize = townSize;

		var rand = new Random( Current.Seed );

		await Current.PlaceWall( position );
		await Current.PlaceProps( PlaceableHouses, rand, position, density, new Vector2( 0f, 0.33f ), true );
		await Current.PlaceProps( PlaceableBigProps, rand, position, density, new Vector2( 0.35f, 0.4f ) );
		await Current.PlaceProps( PlaceableSmallProps, rand, position, density, new Vector2( 0.43f, 0.47f ) );
		await Current.PlaceNPCs( PlaceablePeople, rand, position, density, new Vector2( 0.7f, 1f ) );
		GameMgr.BroadcastTrees( position, 300f * (float)Math.Sqrt( Current.TownSize / 5 ) );

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
