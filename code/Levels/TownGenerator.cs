using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using static Sandbox.CitizenAnimationHelper;

namespace GameJam;

public enum TownType
{
	Camp,
	Village,
	Town
}

public partial class Town : BaseNetworkable
{
	[Net] public bool Generated { get; private set; } = false;
	[Net] public float TownSize { get; set; } = 0f;
	public float TownRadius => 300f * (float)Math.Sqrt( TownSize / 5 );
	public TownType TownType => TownRadius >= 1200f ? ( TownRadius >= 2500f ? TownType.Town : TownType.Village ) : TownType.Camp;
	public float ForestRadius => TownRadius + 1000f;
	[Net] public Vector3 Position { get; set; } = Vector3.Zero;
	public Vector3 MinBounds => Position - new Vector3( TownRadius ).WithZ(0);
	public Vector3 MaxBounds => Position + new Vector3( TownRadius ).WithZ(0);
	public int Seed => TownSize.GetHashCode();
	public Random RNG { get; set; }
	public static List<Entity> TownEntities = new();
	public static List<SceneObject> TownTrees = new();
	public static List<WallObject> TownFences = new();

	public static Dictionary<string, float> PlaceableHousesSmall { get; set; } = new()
	{
		{ "prefabs/raidablebuildings/house_a.prefab", 0.3f },
		{ "prefabs/raidablebuildings/tent_a.prefab", 1f },
		{ "prefabs/raidablebuildings/tent_b.prefab", 0.5f },
	}; 
	
	public static Dictionary<string, float> PlaceableHousesMedium { get; set; } = new()
	{
		{ "prefabs/raidablebuildings/house_a.prefab", 1f },
		{ "prefabs/raidablebuildings/house_b.prefab", 0.3f },
		{ "prefabs/raidablebuildings/house_c.prefab", 0.3f },
		{ "prefabs/raidablebuildings/tent_b.prefab", 0.1f },
	};

	public static Dictionary<string, float> PlaceableHousesBig { get; set; } = new()
	{
		{ "prefabs/raidablebuildings/house_a.prefab", 1.5f },
		{ "prefabs/raidablebuildings/house_b.prefab", 0.5f },
		{ "prefabs/raidablebuildings/house_c.prefab", 0.5f },
		{ "prefabs/raidablebuildings/house_d.prefab", 0.5f },
	};

	public static Dictionary<string, float> PlaceableBigProps { get; set; } = new()
	{
		{ "prefabs/props/stand.prefab", 0.3f },
		{ "prefabs/props/waggon.prefab", 0.5f },
	};

	public static Dictionary<string, float> PlaceableSmallProps { get; set; } = new()
	{
		{ "prefabs/props/box.prefab", 3f },
		{ "prefabs/props/barrel.prefab", 2f },
		{ "prefabs/props/largecrate.prefab", 2f },
		{ "prefabs/props/smallcrate.prefab", 3f },
	};

	public static Dictionary<string, float> PlaceablePeople { get; set; } = new()
	{
		{ "prefabs/npcs/guard.prefab", 1f },
		{ "prefabs/npcs/villager.prefab", 6f },
		{ "prefabs/npcs/woman.prefab", 2f },
		{ "prefabs/npcs/soldier.prefab", 0.1f },
	};

	public static Dictionary<string, float> PlaceableFences { get; set; } = new()
	{
		{ "models/logwall/logwall.vmdl", 130f },
		{ "models/fence/fence.vmdl", 100f },
	};

	public static Dictionary<string, float> PlaceableTrees { get; set; } = new()
	{
		{ "models/trees/shitty_pine_tree.vmdl", 1f },
	};

	public Town() 
	{
		Event.Register( this );
	}

	~Town()
	{
		Event.Unregister( this );
	}

	public static float NoiseValue( float x = 0f, float y = 0f, float scale = 10f )
	{
		return Noise.Fbm( 2, GameMgr.CurrentTown.Seed / 100f + x / scale, GameMgr.CurrentTown.Seed / 100f + y / scale );
	}
	public static float NoiseFBM( float x = 0f, float y = 0f, float scale = 10f )
	{
		return Noise.Fbm( 2, x / scale, y / scale );
	}

	internal static bool TryPlaceProp( Dictionary<string, float> list, Vector3 position, Vector2 threshold, bool lookAtCenter = false  )
	{
		var noise = NoiseValue( currentX * 50f, currentY * 50f );
		var rand = GameMgr.CurrentTown.RNG;

		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var chosenPrefab = WeightedList.RandomKey( rand, list );
			Prefab prefab;

			if ( !ResourceLibrary.TryGet<Prefab>( chosenPrefab, out prefab ) ) return false;

			var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * 25f;
			var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * 25f;
			var chosenPosition = position + new Vector3( randomOffsetX, randomOffsetY, 0 );
			var chosenRotation = lookAtCenter ? Rotation.LookAt( chosenPosition - GameMgr.CurrentTown.Position ) : Rotation.FromYaw( rand.Next( 360 ) );

			var model = GameMgr.PrecachedModels[prefab.Root.GetValue<string>( "Model" )];

			var traceCheck = Trace.Box( model.PhysicsBounds * 1.5f, chosenPosition, chosenPosition )
				.EntitiesOnly()
				.Run();

			if ( traceCheck.Hit ) return false;

			var spawnedEntity = BaseProp.FromPrefab( chosenPrefab );
			if ( spawnedEntity == null ) return false;

			spawnedEntity.Position = chosenPosition;
			spawnedEntity.Rotation = chosenRotation;

			Town.TownEntities.Add( spawnedEntity );
			return true;

		}

		return false;
	}

	internal static bool TryPlaceHouse( Dictionary<string, float> list, Vector3 position, Vector2 threshold, bool lookAtCenter = false )
	{
		var noise = NoiseValue( currentX * 50f, currentY * 50f );
		var rand = GameMgr.CurrentTown.RNG;

		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var chosenPrefab = WeightedList.RandomKey( rand, list );
			Prefab prefab;

			if ( !ResourceLibrary.TryGet<Prefab>( chosenPrefab, out prefab ) ) return false;

			var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * 25f;
			var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * 25f;
			var chosenPosition = position + new Vector3( randomOffsetX, randomOffsetY, 0 );
			var chosenRotation = lookAtCenter ? Rotation.LookAt( chosenPosition - GameMgr.CurrentTown.Position ) : Rotation.FromYaw( rand.Next( 360 ) );

			var model = GameMgr.PrecachedModels[prefab.Root.GetValue<string>( "Model" )];

			var traceCheck = Trace.Box( model.PhysicsBounds * 1.5f, chosenPosition, chosenPosition )
				.EntitiesOnly()
				.Run();

			if ( traceCheck.Hit ) return false;

			var spawnedEntity = RaidableBuilding.FromPrefab( chosenPrefab );
			if ( spawnedEntity == null ) return false;

			spawnedEntity.Position = chosenPosition;
			spawnedEntity.Rotation = chosenRotation;

			Town.TownEntities.Add( spawnedEntity );
			return true;

		}

		return false;
	}

	internal static bool TryPlaceNPC( Dictionary<string, float> list, Vector3 position, Vector2 threshold )
	{
		var noise = NoiseValue( currentX * 50f, currentY * 50f );
		var rand = GameMgr.CurrentTown.RNG;

		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * 25f;
			var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * 25f;
			var chosenPosition = position + new Vector3( randomOffsetX, randomOffsetY, 0 );
			var chosenRotation = Rotation.FromYaw( rand.Next( 360 ) );

			var traceCheck = Trace.Box( new BBox(0f, 50f), chosenPosition, chosenPosition )
				.EntitiesOnly()
				.Run();

			if ( traceCheck.Hit ) return false;

			var spawnedEntity = BaseNPC.FromPrefab( WeightedList.RandomKey( rand, list ) );
			if ( spawnedEntity == null ) return false;

			spawnedEntity.Position = chosenPosition;
			spawnedEntity.Rotation = chosenRotation;

			Town.TownEntities.Add( spawnedEntity );
			return true;

		}

		return false;
	}

	internal static void TryPlaceTree( Dictionary<string, float> list, Vector3 position, float x, float y, Vector2 threshold )
	{
		var noise = NoiseFBM( x, y, 3f );

		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var transform = new Transform( position + new Vector3( x, y, 0 ), Rotation.FromYaw( Game.Random.Int( 360 ) ), Game.Random.Float( 1f, 2f ) );
			var spawnedTree = new SceneObject( Game.SceneWorld, WeightedList.RandomKey( list ), transform );
				
			TownTrees.Add( spawnedTree );
		}
	}

	public static void PlaceTrees()
	{
		foreach( var tree in TownTrees )
		{
			tree.Delete();
		}

		var clearingDistance = GameMgr.CurrentTown.TownRadius + 400f;
		var forestSize = clearingDistance + 1200f;
		var clearingSquared = clearingDistance * clearingDistance;
		var forestSizeSquared = forestSize * forestSize;

		for ( float x = -forestSize; x <= forestSize; x += 70f )
		{
			for ( float y = -forestSize; y <= forestSize; y += 70f )
			{
				var squaredDistance = x * x + y * y;

				if ( squaredDistance < clearingSquared ) continue;
				if ( squaredDistance > forestSizeSquared ) continue;

				TryPlaceTree( PlaceableTrees, GameMgr.CurrentTown.Position, x, y, new Vector2( 0f, 0.43f ) );
			}
		}
	}

	public static void PlaceFences()
	{

		foreach ( var fence in TownFences )
		{
			fence.Delete();
		}

		TownFences.Clear();

		var townDiameter = GameMgr.CurrentTown.TownRadius * 2 + 400f;
		var perimeter = 2 * townDiameter * Math.PI;
		var bestFence = GameMgr.CurrentTown.TownType == TownType.Camp ? PlaceableFences.Last() : PlaceableFences.First();
		var fenceSize = bestFence.Value;
		int fenceCount = (int)Math.Ceiling( perimeter / fenceSize / 2 );
		var mainRoadSize = 60f + GameMgr.CurrentTown.TownRadius / 15f;

		for ( int i = 0; i < fenceCount; i++ )
		{
			var angle = i * fenceSize / (townDiameter / 2);
			var x = townDiameter / 2 * (float)Math.Cos( angle );
			var y = townDiameter / 2 * (float)Math.Sin( angle );
			if ( y < mainRoadSize && y > -mainRoadSize ) continue;

			var fencePosition = GameMgr.CurrentTown.Position + Vector3.Forward * x + Vector3.Right * y;
			var transform = new Transform( fencePosition, Rotation.LookAt( fencePosition - GameMgr.CurrentTown.Position ) );
			var spawnedFence = new WallObject( Game.SceneWorld, bestFence.Key, transform, bestFence.Value );
			TownFences.Add( spawnedFence );
		}
	}

	public static double HousesGenerationProgress { get; set; } = 0d;
	public static double BigPropsGenerationProgress { get; set; } = 0d;
	public static double SmallPropsGenerationProgress { get; set; } = 0d;
	public static double NpcsGenerationProgress { get; set; } = 0d;

	public static bool PlacingHouses => HousesGenerationProgress < 1d && BigPropsGenerationProgress == 0d && IsGenerating;
	public static bool PlacingBigProps => BigPropsGenerationProgress < 1d && HousesGenerationProgress >= 1d && IsGenerating;
	public static bool PlacingSmallProps => SmallPropsGenerationProgress < 1d && BigPropsGenerationProgress >= 1d && IsGenerating;
	public static bool PlacingNpcs => NpcsGenerationProgress < 1d && SmallPropsGenerationProgress >= 1d && IsGenerating;

	public static double GenerationProgress => (HousesGenerationProgress + BigPropsGenerationProgress + SmallPropsGenerationProgress + NpcsGenerationProgress) / 4d;

	public static bool IsGenerating => GenerationProgress < 1d;

	static int currentCheck = -1;
	static int totalRows => (int)(GameMgr.CurrentTown.TownRadius * 2f / 50f);
	static int currentX => currentCheck % totalRows - totalRows / 2;
	static int currentY => (int)(currentCheck / totalRows) - totalRows / 2;
	static TimeUntil nextGenerate = 0f;

	public static void GenerateTown( float townSize )
	{
		GameMgr.CurrentTown?.DeleteTown();

		GameMgr.CurrentTown = new Town();
		GameMgr.CurrentTown.TownSize = townSize;

		currentCheck = -1;
		HousesGenerationProgress = 0d;
		BigPropsGenerationProgress = 0d;
		SmallPropsGenerationProgress = 0d;
		NpcsGenerationProgress = 0d;

		GameMgr.CurrentTown.RNG = new Random( GameMgr.CurrentTown.Seed );

		GameMgr.CurrentTown.Position = GameMgr.CurrentTown.TownType switch
		{
			TownType.Village => new Vector3( 4586f, 452f, 512f ),
			TownType.Town => new Vector3( -4100f, 5414f, 512f ),
			_ => new Vector3( 55f, 292.05f, 512f ),
		};

		var wellEntity = RaidableBuilding.FromPrefab( "prefabs/raidablebuildings/well.prefab" );
		wellEntity.Position = GameMgr.CurrentTown.Position + Vector3.Down * 0.05f;
		wellEntity.Rotation = Rotation.FromYaw( Game.Random.Int( 360 ) );
		Town.TownEntities.Add( wellEntity );

		GameMgr.BroadcastFences();
		GameMgr.BroadcastTrees();
	}

	[Event.Tick.Server]
	public static void GeneratingTown()
	{
		if ( GameMgr.CurrentTown == null ) return;

		var townRadius = GameMgr.CurrentTown.TownRadius;
		var townPosition = GameMgr.CurrentTown.Position;
		var townRadiusSquared = townRadius * townRadius;
		var mainRoadSize = 60f + townRadius / 15f;

		while ( nextGenerate && IsGenerating && !GameMgr.CurrentTown.Generated )
		{
			currentCheck = ( currentCheck + 1 ) % ( totalRows * totalRows );
			var squaredDistance = currentX * 50f * currentX * 50f + currentY * 50f * currentY * 50f;

			if ( HousesGenerationProgress < 1d )
			{
				HousesGenerationProgress = Math.Clamp( HousesGenerationProgress + 1d / (totalRows * totalRows), 0d, 1d );

				if ( squaredDistance > townRadiusSquared ) continue;
				if ( currentY * 50f < mainRoadSize && currentY * 50f > -mainRoadSize ) continue;

				if ( GameMgr.CurrentTown.TownType == TownType.Town )
					if ( TryPlaceHouse( PlaceableHousesBig, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0f, 0.33f ), true ) )
						nextGenerate = Time.Delta / 2f;

				if ( GameMgr.CurrentTown.TownType == TownType.Village )
					if ( TryPlaceHouse( PlaceableHousesMedium, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0f, 0.35f ), true ) )
						nextGenerate = Time.Delta / 2f;

				if ( GameMgr.CurrentTown.TownType == TownType.Camp )
					if ( TryPlaceHouse( PlaceableHousesSmall, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0f, 0.4f ), true ) )
						nextGenerate = Time.Delta / 2f;
			}

			if ( BigPropsGenerationProgress < 1d && HousesGenerationProgress >= 1d )
			{
				BigPropsGenerationProgress = Math.Clamp( BigPropsGenerationProgress + 1d / (totalRows * totalRows), 0d, 1d );

				if ( squaredDistance > townRadiusSquared ) continue;
				if ( currentY * 50f < mainRoadSize && currentY * 50f > -mainRoadSize ) continue;
				
				if ( TryPlaceProp( PlaceableBigProps, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0.44f, 0.45f ) ) )
						nextGenerate = Time.Delta / 2f;
			}

			if ( SmallPropsGenerationProgress < 1d && BigPropsGenerationProgress >= 1d )
			{
				SmallPropsGenerationProgress = Math.Clamp( SmallPropsGenerationProgress + 1d / (totalRows * totalRows), 0d, 1d );

				if ( squaredDistance > townRadiusSquared ) continue;
				if ( currentY * 50f < mainRoadSize && currentY * 50f > -mainRoadSize ) continue;

				if ( TryPlaceProp( PlaceableSmallProps, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0.35f, 0.42f ) ) )
					nextGenerate = Time.Delta / 2f;
			}

			if ( NpcsGenerationProgress < 1d && SmallPropsGenerationProgress >= 1d )
			{
				NpcsGenerationProgress = Math.Clamp( NpcsGenerationProgress + 1d / (totalRows * totalRows), 0d, 1d );

				if ( squaredDistance > townRadiusSquared ) continue;
				if ( currentY * 50f < mainRoadSize && currentY * 50f > -mainRoadSize ) continue;

				if ( TryPlaceNPC( PlaceablePeople, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0.66f, 1.0f ) ) )
					nextGenerate = Time.Delta / 2f;
			}

			//Log.Error( $"Placing {(PlacingHouses ? "Houses" : ( PlacingBigProps ? "Big Props" : ( PlacingSmallProps ? "Small Props" : "NPCs" ) ))} [{Math.Ceiling( GenerationProgress * 100 )}%]" );
		}

		if ( !IsGenerating && !GameMgr.CurrentTown.Generated )
		{
			GameMgr.CurrentTown.Generated = true;
		}
	}

	public RaidableBuilding Throne { get; set; } = null; 

	public static void GenerateEmptyTown( float townSize, bool goldPile = true, bool deleteOld = true )
	{
		if ( deleteOld )
			GameMgr.CurrentTown?.DeleteTown();

		var oldPosition = GameMgr.CurrentTown?.Position ?? new Vector3( 55f, 292.05f, 512f );

		GameMgr.CurrentTown = new Town();
		GameMgr.CurrentTown.TownSize = townSize;

		GameMgr.CurrentTown.Position = GameMgr.CurrentTown.TownType switch
		{
			TownType.Village => new Vector3( 4586f, 452f, 512f ),
			TownType.Town => new Vector3( -4100f, 5414f, 512f ),
			_ => new Vector3( 55f, 292.05f, 512f ),
		};

		GameMgr.CurrentTown.RNG = new Random( GameMgr.CurrentTown.Seed );

		if ( goldPile && deleteOld )
		{
			GameMgr.CurrentTown.Throne = RaidableBuilding.FromPrefab( "prefabs/raidablebuildings/goldpile.prefab" );
			GameMgr.CurrentTown.Throne.Position = GameMgr.CurrentTown.Position + Vector3.Down * 2f;
			Town.TownEntities.Add( GameMgr.CurrentTown.Throne );
		}

		if ( !deleteOld )
		{
			foreach ( var goblin in GameMgr.GoblinArmy )
			{
				var relativePosition = goblin.Position - oldPosition;
				goblin.Position = GameMgr.CurrentTown.Position + relativePosition;
			}
			foreach ( var player in Entity.All.OfType<Lord>() )
			{
				var relativePosition = player.Position - oldPosition;
				player.Position = GameMgr.CurrentTown.Position + relativePosition;
			}
			foreach ( var ent in TownEntities )
			{
				var relativePosition = ent.Position - oldPosition;
				ent.Position = GameMgr.CurrentTown.Position + relativePosition;
			}
		}

		GameMgr.BroadcastFences();
		GameMgr.BroadcastTrees();
		GameMgr.CurrentTown.Generated = true;
	}

	[Event("UpgradeBought")]
	public static void CheckNewTownSize( string identifier )
	{
		if ( identifier.StartsWith( "Village Size" ) )
		{
			GameMgr.LoadVillageSize();
			GenerateEmptyTown( (float)GameMgr.VillageSize, true, false );
		}
	}

	TimeUntil checkFences { get; set; } = 0.2f;

	[Event.Client.Frame]
	public void CheckFences()
	{
		if ( checkFences )
		{
			List<WallObject> toBreak = new();
			foreach ( var fence in TownFences )
			{
				var trace = Trace.Box( WallObject.Box, fence.Transform.PointToWorld( Vector3.Left * fence.Length / 2 ), fence.Transform.PointToWorld( Vector3.Right * fence.Length / 2 ) )
					.EntitiesOnly()
					.WithTag( "Goblins" )
					.Run();

				if ( trace.Hit && fence.IsValid() )
					toBreak.Add( fence );
			}

			for( int i = 0; i < toBreak.Count; i++ )
			{
				toBreak.First().Break();
			}

			checkFences = 0.2f;
		}
	}

	public void DeleteTown()
	{
		foreach ( var entity in TownEntities )
			entity.Delete();
	}

	[ConCmd.Admin( "town" )]
	public static void CreateTown( float townSize = 100f )
	{
		if ( ConsoleSystem.Caller.Pawn is not Lord player )
			return;

		Town.GenerateTown( townSize );
	}
}
