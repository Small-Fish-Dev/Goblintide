﻿using Sandbox;
using System.IO;

namespace GameJam;

partial class GameMgr
{
	public const bool ENABLE_SAVESYSTEM = true;
	public const string SAVE_PATH = "./save.dat";

	/// <summary>
	/// Are we done loading the game save?
	/// </summary>
	public static bool Loaded
	{
		get => Instance.loaded;
		private set
		{
			Instance.loaded = value;
		}
	}

	[Net] private bool loaded { get; set; } = false;

	#region Goblin Persistence
	private static void goblinPersist( object method )
	{
		// Handle saving first.
		if ( method is BinaryWriter writer )
		{
			var goblins = GoblinArmy.ToArray();

			writer.Write( goblins.Length );
			foreach ( var goblin in goblins )
			{
				writer.Write( goblin.Name.ToLower() );
				writer.Write( goblin.DisplayName );
				writer.Write( goblin.GetMaterialGroup() );
				writer.Write( goblin.Weapon?.Name ?? "null" );
				writer.Write( goblin.Armor?.Name ?? "null" );
			}

			return;
		}

		// Handle loading save.
		if ( method is BinaryReader reader )
		{
			var count = reader.ReadInt32();
			for ( int i = 0; i < count; i++ )
			{
				var prefabName = reader.ReadString();
				var name = reader.ReadString();
				var materialGroup = reader.ReadInt32();
				var weaponName = reader.ReadString();
				var armorName = reader.ReadString();
				var npc = BaseNPC.FromPrefab( $"prefabs/npcs/{prefabName}.prefab" );
				if ( npc == null || !npc.IsValid )
					continue;

				GoblinArmy.Add( npc );

				npc.Position = Lord.Position + Vector3.Random.WithZ( 0 ) * 100f;
				npc.DisplayName = name;
				npc.SetMaterialGroup( materialGroup );
				if ( weaponName != "null" )
				{
					var weapon = BaseItem.FromPrefab( $"prefabs/items/{weaponName}.prefab" );

					if ( weapon.IsValid() )
					{
						npc.Equip( weapon );
					}
				}

				if ( armorName != "null" )
				{
					var armor = BaseItem.FromPrefab( $"prefabs/items/{armorName}.prefab" );
					if ( armor.IsValid() )
					{
						npc.Equip( armor );
					}
				}
			}
		}
	}
	#endregion

	#region Resources Persistence
	private static void resourcesPersist( object method )
	{
		if ( method is BinaryWriter writer )
		{
			writer.Write( TotalWood );
			writer.Write( TotalGold );
			writer.Write( TotalIQ );
			writer.Write( MaxIQ );
			writer.Write( TotalFood );
			writer.Write( TotalWomen );
			writer.Write( TotalEnergy );
			writer.Write( EnergyRechargeRate );
			writer.Write( LastEnergyUpdate );

			return;
		}

		// Handle loading save.
		if ( method is BinaryReader reader )
		{
			TotalWood = reader.ReadInt32();
			TotalGold = reader.ReadInt32();
			TotalIQ = reader.ReadInt32();
			MaxIQ = reader.ReadInt32();
			TotalFood = reader.ReadInt32();
			TotalWomen = reader.ReadInt32();
			TotalEnergy = reader.ReadDouble();
			EnergyRechargeRate = reader.ReadDouble();
			LastEnergyUpdate = reader.ReadInt64();
		}
	}
	#endregion	
	
	#region World Map Persistence
	private static void worldMapPersist( object method )
	{
		// Handle saving first.
		if ( method is BinaryWriter writer )
		{
			int count = WorldMapHost.Entries.Count();
			writer.Write( count );

			foreach ( WorldMapHost.Generator entry in WorldMapHost.Entries )
			{
				writer.Write( entry.MapPosition );
				writer.Write( entry.Size );
			}
			return;
		}

		// Handle loading save.
		if ( method is BinaryReader reader )
		{
			int count = reader.ReadInt32();

			for ( int i = 0; i < count; i++ )
			{
				var position = reader.ReadVector2();
				var size = reader.ReadDouble();
				new WorldMapHost.Generator( position, size );
			}
		}
	}
#endregion

	#region Lord Persistence
	// TODO: Save Lord upgrades.
	private static void lordPersist( object method )
	{
		// Handle saving first.
		if ( method is BinaryWriter writer )
		{
			writer.Write( Lord.Upgrades.Count );
			foreach ( var upgrade in Lord.Upgrades )
				writer.Write( upgrade );

			return;
		}

		// Handle loading save.
		if ( method is BinaryReader reader )
		{
			Lord.Upgrades.Clear();

			var count = reader.ReadInt32();
			for ( int i = 0; i < count; i++ )
			{
				var identifier = reader.ReadString();
				Lord.AddUpgrade( identifier );
			}

			Lord.CombineUpgrades();
		}
	}
	#endregion

	#region Village Persistence
	// TODO: Save all built structures and village size.
	private static void villagePersist( object method )
	{
		// Handle saving first.
		if ( method is BinaryWriter writer )
		{
			writer.Write( VillageState.Structures.Count );
			foreach ( var building in VillageState.Structures )
			{
				writer.Write( building.PrefabName );
				writer.Write( building.Position );
			}

			return;
		}

		// Handle loading save.
		if ( method is BinaryReader reader )
		{
			VillageState.Structures.Clear();

			var count = reader.ReadInt32();
			for ( int i = 0; i < count; i++ )
			{
				var entry = new BuildingEntry()
				{
					PrefabName = reader.ReadString(),
					Position = reader.ReadVector3()
				};

				VillageState.TrySpawnStructure( entry );
			}
		}
	}
	#endregion

	public static async Task<bool> GenerateSave( bool force = false )
	{
		if ( !ENABLE_SAVESYSTEM && !force )
			return true;

		if ( Lord == null || !Lord.IsValid )
			return false;

		// Initialize stream and writer.
		using var stream = FileSystem.Data.OpenWrite( SAVE_PATH, FileMode.OpenOrCreate );
		using var writer = new BinaryWriter( stream );

		// Save all data.
		lordPersist( writer );
		resourcesPersist( writer );
		worldMapPersist( writer );
		villagePersist( writer );
		goblinPersist( writer );
		
		// Close the stream and finish.
		stream.Close();

		return true;
	}

	public static async Task<bool> LoadSave( bool force = false )
	{
		if ( !ENABLE_SAVESYSTEM && !force )
			return true;

		if ( Lord == null || !Lord.IsValid )
			return false;

		// Check if we can load a save.
		if ( !FileSystem.Data.FileExists( SAVE_PATH ) )
		{
			// Do we need to do initial stuff if the save doesn't exist?
			Loaded = true;
			return false;
		}

		// Initialize stream and reader.
		using var stream = FileSystem.Data.OpenRead( SAVE_PATH );
		using var reader = new BinaryReader( stream );
		var shouldDelete = false;

		try
		{
			// Read all data and act according to it.
			lordPersist( reader );
			resourcesPersist( reader );
			worldMapPersist( reader );
			villagePersist( reader );
			goblinPersist( reader );
		}
		catch ( Exception ex )
		{
			Log.Error( $"Failed to load save properly." );
			shouldDelete = true;
		}

		LoadVillageSize();
		// Tell everyone that we're done loading.
		Loaded = true;

		// Close stream and finish.
		stream.Close();

		// Delete save.
		if ( shouldDelete )
			FileSystem.Data.DeleteFile( SAVE_PATH );

		return true;
	}

	public static void LoadVillageSize()
	{
		if ( Lord.CombinedUpgrades == null ) return;

		if ( Lord.CombinedUpgrades.VillageSize > 0f )
		{
			GameMgr.VillageSize = 5d + Lord.CombinedUpgrades.VillageSize;
		}
	}

	[ConCmd.Server]
	public static void RequestSave()
	{
		if ( ConsoleSystem.Caller.Pawn != Lord )
			return;

		GameTask.RunInThreadAsync( () => GenerateSave( true ) );
	}

	TimeUntil nextSave { get; set; } = 15f;
	[Event.Tick.Server]
	public void AutoSave()
	{
		if ( nextSave )
		{
			nextSave = 15f;

			if ( GameMgr.State is VillageState )
			{
				Log.Info( "Autosaving..." );
				GameTask.RunInThreadAsync( async () =>
				{
					await GenerateSave( true );
					Log.Info( "Finished autosaving" );
				} );
			}
		}
	}

	[ConCmd.Server]
	public static void RequestLoad()
	{
		if ( ConsoleSystem.Caller.Pawn != Lord )
			return;

		LoadSave( true );
	}
}
