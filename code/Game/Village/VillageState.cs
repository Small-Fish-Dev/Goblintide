using Sandbox.Internal;
using System.Diagnostics;

namespace GameJam;

public struct BuildingEntry
{
	public string PrefabName { get; set; }
	public Vector3 Position { get; set; }
}

public partial class VillageState : GameState
{
	/// <summary>
	/// All structures you own.
	/// </summary>
	public static List<BuildingEntry> Structures { get; } = new();

	VillageHUD hud;
	static VillageState instance;

	public override void Initialize()
	{
		instance?.hud?.Delete( true );
		instance = this;

		if ( Game.IsClient )
		{
			hud = HUD.Instance.AddChild<VillageHUD>();
			GameMgr.Music.Stop();
			GameMgr.Music = Sound.FromScreen( "sounds/music/village_song.sound" );
			return;
		}

		Town.GenerateEmptyTown( (float)GameMgr.VillageSize );
		GameMgr.LoadSave( true, true );
		GameMgr.Lord.Position = GameMgr.CurrentTown.Throne.Position + 50f;
		GameMgr.GoblinArmyEnabled( true );
		GameMgr.PlaceGoblinArmy( true );

		foreach( var goblin in GameMgr.GoblinArmy )
		{
			goblin.BaseDiligency = 1;
		}

		if ( Game.IsServer )
		{
			if ( GameMgr.Tutorial )
			{
				GameMgr.DoTutorial();
			}
		}
	}

	public override void Changed( GameState state )
	{
		instance?.hud?.Delete( true );
		
		if ( Game.IsServer )
		{

			GameMgr.GenerateSave( true );

			foreach ( var goblin in GameMgr.GoblinArmy )
			{
				goblin.BaseDiligency = goblin.RootPrefab.GetValue<float>("BaseDiligency");
			}
		}
	}

	public static bool TrySpawnStructure( BuildingEntry entry, out BaseStructure structure )
	{
		structure = null;

		if ( !Structures.Contains( entry ) )
			Structures.Add( entry );

		if ( instance == null )
			return false;

		structure = BaseStructure.FromPrefab( entry.PrefabName );

		if ( structure == null )
			return false;

		var pos = new Vector3( entry.Position.x, entry.Position.y, GameMgr.CurrentTown.Position.z );
		structure.Position = pos;

		var rotation = Rotation.LookAt( pos - GameMgr.CurrentTown.Position );
		structure.Rotation = Rotation.FromYaw( rotation.Yaw() );
		structure.Entry = entry;

		return true;
	}

	[Event.Tick.Server]
	private void onTick()
	{
		if ( GameMgr.Lord == null )
			return;

		// Block movement if in overview mode.
		GameMgr.Lord.BlockMovement = GameMgr.Lord.Overview;

		if ( GameMgr.Lord.Position.Distance( GameMgr.CurrentTown.Position ) >= GameMgr.CurrentTown.ForestRadius - 200f )
		{
			GameMgr.Lord.Position = GameMgr.CurrentTown.Throne.Position + 20f;
		}

		if ( GameMgr.CurrentTown?.Throne?.IsValid() ?? false )
			GameMgr.CurrentTown.Throne.Position = GameMgr.CurrentTown.Position + Vector3.Down * 2f + Vector3.Up * Math.Min( (float)Math.Sqrt( GameMgr.TotalGold ), 100f );
	}

	[ConCmd.Admin( "SpawnStructure" )]
	public static void SpawnStructure( string name, float x = 0, float y = 0 )
	{
		TrySpawnStructure( new() 
		{
			PrefabName = name, 
			Position = new Vector3( x, y )
		}, out var structure );
	}
}
