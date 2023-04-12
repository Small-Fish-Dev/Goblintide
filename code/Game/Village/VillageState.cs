using Sandbox.Internal;
using System.Diagnostics;

namespace GoblinGame;

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
			Goblintide.Music.Stop();
			Goblintide.Music = Sound.FromScreen( "sounds/music/village_song.sound" );
			return;
		}

		Town.GenerateEmptyTown( (float)Goblintide.VillageSize );
		Goblintide.LoadSave( true, true );
		Goblintide.Lord.Position = Goblintide.CurrentTown.Throne.Position + 50f;
		Goblintide.Lord.HitPoints = Goblintide.Lord.MaxHitPoints;
		Goblintide.GoblinArmyEnabled( true );
		Goblintide.PlaceGoblinArmy( true );

		foreach( var goblin in Goblintide.GoblinArmy )
		{
			goblin.BaseDiligency = 1;
		}

		if ( Game.IsServer )
		{
			if ( Goblintide.Tutorial )
			{
				Goblintide.DoTutorial();
			}
		}
	}

	public override void Changed( GameState state )
	{
		instance?.hud?.Delete( true );
		
		if ( Game.IsServer )
		{

			Goblintide.GenerateSave( true );

			foreach ( var structure in Entity.All.OfType<BaseStructure>() )
				structure.Delete();

			foreach ( var goblin in Goblintide.GoblinArmy )
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

		var pos = new Vector3( entry.Position.x, entry.Position.y, Goblintide.CurrentTown.Position.z );
		structure.Position = pos;

		var rotation = Rotation.LookAt( pos - Goblintide.CurrentTown.Position );
		structure.Rotation = Rotation.FromYaw( rotation.Yaw() );
		structure.Entry = entry;

		return true;
	}

	[Event.Tick.Server]
	private void onTick()
	{
		if ( Goblintide.Lord == null )
			return;

		// Block movement if in overview mode.
		Goblintide.Lord.BlockMovement = Goblintide.Lord.Overview;

		if ( Goblintide.Lord.Position.Distance( Goblintide.CurrentTown.Position ) >= Goblintide.CurrentTown.ForestRadius - 200f )
		{
			Goblintide.Lord.Position = Goblintide.CurrentTown.Throne.Position + 20f;
		}

		if ( Goblintide.CurrentTown?.Throne?.IsValid() ?? false )
			Goblintide.CurrentTown.Throne.Position = Goblintide.CurrentTown.Position + Vector3.Down * 2f + Vector3.Up * Math.Min( (float)Math.Sqrt( Goblintide.TotalGold ), 100f );
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
