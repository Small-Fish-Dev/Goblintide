using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using static Sandbox.CitizenAnimationHelper;

namespace GoblinGame;

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
	public GridAStar.Grid Grid { get; set; }
	public bool EmptyTown { get; set; } = false;
	public float TownRadius => 300f * (float)Math.Sqrt( TownSize / 5 );
	public TownType TownType => TownRadius >= 1200f ? ( TownRadius >= 2500f ? TownType.Town : TownType.Village ) : TownType.Camp;
	public float ForestRadius => TownRadius + 1000f;
	[Net] public Vector3 Position { get; set; } = Vector3.Zero;
	public Vector3 MinBounds => Position - new Vector3( ForestRadius ).WithZ(0);
	public Vector3 MaxBounds => Position + new Vector3( ForestRadius ).WithZ(0);
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
		{ "models/trees/shitty_pine_tree2.vmdl", 1f },
		{ "models/trees/george_bush.vmdl", 0.25f },
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
		return Noise.Fbm( 2, Goblintide.CurrentTown.Seed / 100f + x / scale, Goblintide.CurrentTown.Seed / 100f + y / scale );
	}
	public static float NoiseFBM( float x = 0f, float y = 0f, float scale = 10f )
	{
		return Noise.Fbm( 2, x / scale, y / scale );
	}

	internal static bool TryPlaceProp( Dictionary<string, float> list, Vector3 position, Vector2 threshold, bool lookAtCenter = false  )
	{
		var noise = NoiseValue( currentX * 50f, currentY * 50f );
		var rand = Goblintide.CurrentTown.RNG;

		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var chosenPrefab = WeightedList.RandomKey( rand, list );
			Prefab prefab;

			if ( !ResourceLibrary.TryGet<Prefab>( chosenPrefab, out prefab ) ) return false;

			var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * 25f;
			var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * 25f;
			var chosenPosition = position + new Vector3( randomOffsetX, randomOffsetY, 0 );
			var chosenRotation = lookAtCenter ? Rotation.LookAt( chosenPosition - Goblintide.CurrentTown.Position ) : Rotation.FromYaw( rand.Next( 360 ) );

			var model = Goblintide.PrecachedModels[prefab.Root.GetValue<string>( "Model" )];

			var traceCheck = Trace.Box( model.PhysicsBounds * 1.5f, chosenPosition, chosenPosition )
				.EntitiesOnly()
				.Run();

			if ( traceCheck.Hit ) return false;

			var spawnedEntity = BaseProp.FromPrefab( chosenPrefab );
			if ( spawnedEntity == null ) return false;

			spawnedEntity.Position = chosenPosition;
			spawnedEntity.Rotation = chosenRotation;

			spawnedEntity.Tags.Add( "GridBlocker" );

			Town.TownEntities.Add( spawnedEntity );
			return true;

		}

		return false;
	}

	internal static bool TryPlaceHouse( Dictionary<string, float> list, Vector3 position, Vector2 threshold, bool lookAtCenter = false )
	{
		var noise = NoiseValue( currentX * 50f, currentY * 50f );
		var rand = Goblintide.CurrentTown.RNG;

		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var chosenPrefab = WeightedList.RandomKey( rand, list );
			Prefab prefab;

			if ( !ResourceLibrary.TryGet<Prefab>( chosenPrefab, out prefab ) ) return false;

			var randomOffsetX = (float)(rand.NextDouble() * 2f - 0.5f) * 25f;
			var randomOffsetY = (float)(rand.NextDouble() * 2f - 0.5f) * 25f;
			var chosenPosition = position + new Vector3( randomOffsetX, randomOffsetY, 0 );
			var chosenRotation = lookAtCenter ? Rotation.LookAt( chosenPosition - Goblintide.CurrentTown.Position ) : Rotation.FromYaw( rand.Next( 360 ) );

			var model = Goblintide.PrecachedModels[prefab.Root.GetValue<string>( "Model" )];

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
		var rand = Goblintide.CurrentTown.RNG;

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
		var noise = NoiseFBM( x, y, 7f );

		if ( noise >= threshold.x && noise <= threshold.y )
		{
			var transform = new Transform( position + new Vector3( x, y, 0 ), Rotation.FromYaw( Game.Random.Int( 360 ) ), Game.Random.Float( 1f, 2f ) );
			var spawnedTree = new SceneObject( Game.SceneWorld, WeightedList.RandomKey( list ), transform );
				
			TownTrees.Add( spawnedTree );
		}
	}

	public async static Task<bool> GenerateGrid()
	{
		var position = Goblintide.CurrentTown.Position;
		var bounds = new BBox( Goblintide.CurrentTown.MinBounds + Vector3.Down * 999f, Goblintide.CurrentTown.MaxBounds + Vector3.Up * 999f );
		Goblintide.CurrentTown.Grid = await GridAStar.Grid.Create( position, bounds, new Rotation(), widthClearance: 12f, heightClearance: 40f, save: false, cylinder: true, worldOnly: false );
		return true;
	}

	public static void PlaceTrees()
	{
		foreach( var tree in TownTrees )
		{
			tree.Delete();
		}

		var clearingDistance = Goblintide.CurrentTown.TownRadius + 400f;
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

				TryPlaceTree( PlaceableTrees, Goblintide.CurrentTown.Position, x, y, new Vector2( 0f, 0.46f ) );
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

		var townDiameter = Goblintide.CurrentTown.TownRadius * 2 + 400f;
		var perimeter = 2 * townDiameter * Math.PI;
		var bestFence = Goblintide.CurrentTown.TownType == TownType.Camp ? PlaceableFences.Last() : PlaceableFences.First();
		var fenceSize = bestFence.Value;
		int fenceCount = (int)Math.Ceiling( perimeter / fenceSize / 2 );
		var mainRoadSize = 60f + Goblintide.CurrentTown.TownRadius / 15f;

		for ( int i = 0; i < fenceCount; i++ )
		{
			var angle = i * fenceSize / (townDiameter / 2);
			var x = townDiameter / 2 * (float)Math.Cos( angle );
			var y = townDiameter / 2 * (float)Math.Sin( angle );
			if ( y < mainRoadSize && y > -mainRoadSize ) continue;

			var fencePosition = Goblintide.CurrentTown.Position + Vector3.Forward * x + Vector3.Right * y;
			var transform = new Transform( fencePosition, Rotation.LookAt( fencePosition - Goblintide.CurrentTown.Position ) );
			var spawnedFence = new WallObject( Game.SceneWorld, bestFence.Key, transform, bestFence.Value );
			TownFences.Add( spawnedFence );
		}
	}

	[Net] public double HousesGenerationProgress { get; set; } = 0d;
	[Net] public double GridGenerationProgress { get; set; } = 0d;
	[Net] public bool GridBlockersPlaced { get; set; } = false;
	[Net] public double BigPropsGenerationProgress { get; set; } = 0d;
	[Net] public double SmallPropsGenerationProgress { get; set; } = 0d;
	[Net] public double NpcsGenerationProgress { get; set; } = 0d;

	public static bool PlacingHouses => Goblintide.CurrentTown.HousesGenerationProgress < 1d && Goblintide.CurrentTown.BigPropsGenerationProgress == 0d && IsGenerating;
	public static bool GeneratingGrid => Goblintide.CurrentTown.GridGenerationProgress < 1d && Goblintide.CurrentTown.HousesGenerationProgress >= 1d && IsGenerating;
	public static bool PlacingBigProps => Goblintide.CurrentTown.BigPropsGenerationProgress < 1d && Goblintide.CurrentTown.GridGenerationProgress >= 1d && IsGenerating;
	public static bool PlacingSmallProps => Goblintide.CurrentTown.SmallPropsGenerationProgress < 1d && Goblintide.CurrentTown.BigPropsGenerationProgress >= 1d && IsGenerating;
	public static bool PlacingNpcs => Goblintide.CurrentTown.NpcsGenerationProgress < 1d && Goblintide.CurrentTown.SmallPropsGenerationProgress >= 1d && IsGenerating;

	public static double GenerationProgress => (Goblintide.CurrentTown.HousesGenerationProgress + Goblintide.CurrentTown.GridGenerationProgress + Goblintide.CurrentTown.BigPropsGenerationProgress + Goblintide.CurrentTown.SmallPropsGenerationProgress + Goblintide.CurrentTown.NpcsGenerationProgress) / 5d;
	public static string GenerationText => $"Placing {(PlacingHouses ? "Houses" : ( PlacingBigProps ? "Big Props" : ( PlacingSmallProps ? "Small Props" : "NPCs" ) ))}... [{Math.Ceiling( GenerationProgress * 100 )}%]";

	public static bool IsGenerating => GenerationProgress < 1d;

	static int currentCheck = -1;
	static int totalRows => (int)(Goblintide.CurrentTown.TownRadius * 2f / 50f);
	static int currentX => currentCheck % totalRows - totalRows / 2;
	static int currentY => (int)(currentCheck / totalRows) - totalRows / 2;
	static TimeUntil nextGenerate = 0f;

	public static void GenerateTown( float townSize )
	{
		Goblintide.CurrentTown?.DeleteTown();

		Goblintide.CurrentTown = new Town();
		Goblintide.CurrentTown.TownSize = townSize;

		currentCheck = -1;
		Goblintide.CurrentTown.HousesGenerationProgress = 0d;
		Goblintide.CurrentTown.GridGenerationProgress = 0d;
		Goblintide.CurrentTown.GridBlockersPlaced = false;
		Goblintide.CurrentTown.BigPropsGenerationProgress = 0d;
		Goblintide.CurrentTown.SmallPropsGenerationProgress = 0d;
		Goblintide.CurrentTown.NpcsGenerationProgress = 0d;

		Goblintide.CurrentTown.RNG = new Random( Goblintide.CurrentTown.Seed );

		Goblintide.CurrentTown.Position = Goblintide.CurrentTown.TownType switch
		{
			TownType.Village => new Vector3( 4586f, 452f, 512f ),
			TownType.Town => new Vector3( -4100f, 5414f, 512f ),
			_ => new Vector3( 55f, 292.05f, 512f ),
		};

		var wellEntity = RaidableBuilding.FromPrefab( "prefabs/raidablebuildings/well.prefab" );
		wellEntity.Position = Goblintide.CurrentTown.Position + Vector3.Down * 0.05f;
		wellEntity.Rotation = Rotation.FromYaw( Game.Random.Int( 360 ) );
		Town.TownEntities.Add( wellEntity );

		Goblintide.BroadcastFences();
		Goblintide.BroadcastTrees();
	}

	[Event.Tick.Server]
	public static void GeneratingTown()
	{
		if ( Goblintide.CurrentTown == null ) return;
		if ( Goblintide.CurrentTown.EmptyTown ) return;

		var townRadius = Goblintide.CurrentTown.TownRadius;
		var townPosition = Goblintide.CurrentTown.Position;
		var townRadiusSquared = townRadius * townRadius;
		var mainRoadSize = 60f + townRadius / 15f;

		while ( nextGenerate && IsGenerating && !Goblintide.CurrentTown.Generated )
		{
			currentCheck = ( currentCheck + 1 ) % ( totalRows * totalRows );
			var squaredDistance = currentX * 50f * currentX * 50f + currentY * 50f * currentY * 50f;

			if ( Goblintide.CurrentTown.HousesGenerationProgress < 1d )
			{
				Goblintide.CurrentTown.HousesGenerationProgress = Math.Clamp( Goblintide.CurrentTown.HousesGenerationProgress + 1d / (totalRows * totalRows), 0d, 1d );

				if ( squaredDistance > townRadiusSquared ) continue;
				if ( currentY * 50f < mainRoadSize && currentY * 50f > -mainRoadSize ) continue;

				if ( Goblintide.CurrentTown.TownType == TownType.Town )
					if ( TryPlaceHouse( PlaceableHousesBig, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0f, 0.33f ), true ) )
						nextGenerate = Time.Delta / 2f;

				if ( Goblintide.CurrentTown.TownType == TownType.Village )
					if ( TryPlaceHouse( PlaceableHousesMedium, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0f, 0.35f ), true ) )
						nextGenerate = Time.Delta / 2f;

				if ( Goblintide.CurrentTown.TownType == TownType.Camp )
					if ( TryPlaceHouse( PlaceableHousesSmall, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0f, 0.4f ), true ) )
						nextGenerate = Time.Delta / 2f;
			}

			if ( Goblintide.CurrentTown.GridGenerationProgress < 1d && Goblintide.CurrentTown.HousesGenerationProgress >= 1d )
			{
				if ( Goblintide.CurrentTown.GridGenerationProgress <= 0d )
				{
					Goblintide.CurrentTown.GridGenerationProgress = 0.5d;

					GameTask.RunInThreadAsync( async () =>
					{
						await GenerateGrid();
						Goblintide.CurrentTown.GridGenerationProgress = 1d;
					} );
				}
			}

			if ( Goblintide.CurrentTown.BigPropsGenerationProgress < 1d && Goblintide.CurrentTown.GridGenerationProgress >= 1d )
			{
				Goblintide.CurrentTown.BigPropsGenerationProgress = Math.Clamp( Goblintide.CurrentTown.BigPropsGenerationProgress + 1d / (totalRows * totalRows), 0d, 1d );

				if ( squaredDistance > townRadiusSquared ) continue;
				if ( currentY * 50f < mainRoadSize && currentY * 50f > -mainRoadSize ) continue;

				if ( TryPlaceProp( PlaceableBigProps, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0.44f, 0.45f ) ) )
					nextGenerate = Time.Delta / 2f;
			}

			if ( Goblintide.CurrentTown.SmallPropsGenerationProgress < 1d && Goblintide.CurrentTown.BigPropsGenerationProgress >= 1d )
			{
				Goblintide.CurrentTown.SmallPropsGenerationProgress = Math.Clamp( Goblintide.CurrentTown.SmallPropsGenerationProgress + 1d / (totalRows * totalRows), 0d, 1d );

				if ( squaredDistance > townRadiusSquared ) continue;
				if ( currentY * 50f < mainRoadSize && currentY * 50f > -mainRoadSize ) continue;

				if ( TryPlaceProp( PlaceableSmallProps, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0.35f, 0.42f ) ) )
					nextGenerate = Time.Delta / 2f;
			}

			if ( Goblintide.CurrentTown.SmallPropsGenerationProgress >= 1d )
			{
				if ( !Goblintide.CurrentTown.GridBlockersPlaced )
				{
					Goblintide.CurrentTown.Grid.CheckOccupancy( "GridBlocker" );
					Goblintide.CurrentTown.GridBlockersPlaced = true;
				}
			}

			if ( Goblintide.CurrentTown.NpcsGenerationProgress < 1d && Goblintide.CurrentTown.SmallPropsGenerationProgress >= 1d )
			{
				Goblintide.CurrentTown.NpcsGenerationProgress = Math.Clamp( Goblintide.CurrentTown.NpcsGenerationProgress + 1d / (totalRows * totalRows), 0d, 1d );

				if ( squaredDistance > townRadiusSquared ) continue;
				if ( currentY * 50f < mainRoadSize && currentY * 50f > -mainRoadSize ) continue;

				if ( TryPlaceNPC( PlaceablePeople, townPosition + new Vector3( currentX * 50f, currentY * 50f ), new Vector2( 0.66f, 1.0f ) ) )
					nextGenerate = Time.Delta / 2f;
			}

		}

		if ( !IsGenerating && !Goblintide.CurrentTown.Generated )
		{
			Goblintide.CurrentTown.Generated = true;
		}
	}

	public RaidableBuilding Throne { get; set; } = null; 

	public static void GenerateEmptyTown( float townSize, bool goldPile = true, bool deleteOld = true )
	{
		if ( deleteOld )
			Goblintide.CurrentTown?.DeleteTown();

		var oldPosition = Goblintide.CurrentTown?.Position ?? new Vector3( 55f, 292.05f, 512f );

		Goblintide.CurrentTown = new Town();
		Goblintide.CurrentTown.EmptyTown = true;
		Goblintide.CurrentTown.TownSize = townSize;

		Goblintide.CurrentTown.Position = Goblintide.CurrentTown.TownType switch
		{
			TownType.Village => new Vector3( 4586f, 452f, 512f ),
			TownType.Town => new Vector3( -4100f, 5414f, 512f ),
			_ => new Vector3( 55f, 292.05f, 512f ),
		};

		Goblintide.CurrentTown.RNG = new Random( Goblintide.CurrentTown.Seed );

		if ( goldPile && deleteOld )
		{
			Goblintide.CurrentTown.Throne = RaidableBuilding.FromPrefab( "prefabs/raidablebuildings/goldpile.prefab" );
			Goblintide.CurrentTown.Throne.Position = Goblintide.CurrentTown.Position + Vector3.Down * 2f;
			Town.TownEntities.Add( Goblintide.CurrentTown.Throne );
		}

		if ( deleteOld )
		{
			foreach ( var goblin in Goblintide.GoblinArmy )
			{
				var relativePosition = goblin.Position - oldPosition;
				goblin.Position = Goblintide.CurrentTown.Position + relativePosition;
			}
			foreach ( var player in Entity.All.OfType<Lord>() )
			{
				var relativePosition = player.Position - oldPosition;
				player.Position = Goblintide.CurrentTown.Position + relativePosition;
			}
			foreach ( var structure in Entity.All.OfType<BaseStructure>() )
			{
				var relativePosition = structure.Position - oldPosition;
				structure.Position = Goblintide.CurrentTown.Position + relativePosition;
			}
			foreach ( var ent in TownEntities )
			{
				if ( !ent.IsValid() ) continue;
				var relativePosition = ent.Position - oldPosition;
				ent.Position = Goblintide.CurrentTown.Position + relativePosition;
			}
		}

		Goblintide.BroadcastFences();
		Goblintide.BroadcastTrees();
		Goblintide.CurrentTown.Generated = true;
	}

	[Event("UpgradeBought")]
	public static void CheckNewTownSize( string identifier )
	{
		if ( identifier.StartsWith( "Village Size" ) )
		{
			Goblintide.LoadVillageSize();
			GenerateEmptyTown( (float)Goblintide.VillageSize, true, true );
		}

		Goblintide.GenerateSave( true );
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
