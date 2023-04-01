using Sandbox;
using System.IO;

namespace GameJam;

partial class GameMgr
{
	public const bool ENABLE_SAVESYSTEM = false;
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
	// TODO: Save & load equipment.
	private static void goblinPersist( object method )
	{
		// Handle saving first.
		if ( method is BinaryWriter writer )
		{
			var goblins = Entity.All.OfType<BaseNPC>()
				.Where( npc => npc.Faction == FactionType.Goblins )
				.ToArray();

			writer.Write( goblins.Length );
			for ( int i = 0; i < goblins.Length; i++ )
			{
				var goblin = goblins[i];
				if ( goblin == null || !goblin.IsValid )
					continue;

				writer.Write( goblin.Name.ToLower() );
				writer.Write( goblin.DisplayName );
				Log.Error( $"persisted {goblin.Name}" );
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
				var npc = BaseNPC.FromPrefab( prefabName );
				if ( npc == null || !npc.IsValid )
					continue;

				npc.Position = Lord.Position + Vector3.Random.WithZ( 0 ) * 100f;
				npc.DisplayName = name;
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
			return;
		}

		// Handle loading save.
		if ( method is BinaryReader reader )
		{
			
		}
	}
	#endregion

	#region GameState Persistence
	// TODO: Save WorldMap state & progress.
	private static void gameStatePersist( object method )
	{
		// Handle saving first.
		if ( method is BinaryWriter writer )
		{
			return;
		}

		// Handle loading save.
		if ( method is BinaryReader reader )
		{

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
			return;
		}

		// Handle loading save.
		if ( method is BinaryReader reader )
		{

		}
	}
	#endregion

	public static async Task<bool> GenerateSave( bool force = false )
	{
		if ( !ENABLE_SAVESYSTEM && !force )
			return true;

		// Initialize stream and writer.
		using var stream = FileSystem.Data.OpenWrite( SAVE_PATH, FileMode.OpenOrCreate );
		using var writer = new BinaryWriter( stream );

		// Save all data.
		gameStatePersist( writer );
		lordPersist( writer );
		goblinPersist( writer );
		villagePersist( writer );

		// Close the stream and finish.
		stream.Close();

		return true;
	}

	public static async Task<bool> LoadSave( bool force = false )
	{
		if ( !ENABLE_SAVESYSTEM && !force )
			return true;

		// Check if we can load a save.
		if ( !FileSystem.Data.FileExists( SAVE_PATH ) )
		{
			// Do we need to do initial stuff if the save doesn't exist?
			return false;
		}

		// Initialize stream and reader.
		using var stream = FileSystem.Data.OpenRead( SAVE_PATH );
		using var reader = new BinaryReader( stream );

		try
		{
			// Read all data and act according to it.
			gameStatePersist( reader );
			lordPersist( reader );
			goblinPersist( reader );
			villagePersist( reader );
		}
		catch ( Exception ex )
		{
			Log.Error( $"Failed to load save properly." );
			FileSystem.Data.DeleteFile( SAVE_PATH );
		}
		
		// Tell everyone that we're done loading.
		Loaded = true;

		return true;
	}

	[ConCmd.Server]
	public static void RequestSave()
	{
		if ( ConsoleSystem.Caller.Pawn != Lord )
			return;

		GameTask.RunInThreadAsync( () => GenerateSave( true ) );
	}

	[ConCmd.Server]
	public static void RequestLoad()
	{
		if ( ConsoleSystem.Caller.Pawn != Lord )
			return;

		LoadSave( true );
	}
}
