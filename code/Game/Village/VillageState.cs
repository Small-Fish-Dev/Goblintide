﻿namespace GameJam;

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
		instance = this;

		if ( Game.IsClient )
		{
			hud = HUD.Instance.AddChild<VillageHUD>();
			return;
		}

		Town.GenerateEmptyTown( (float)GameMgr.VillageSize );
		GameMgr.Lord.Position = GameMgr.CurrentTown.Throne.Position + 50f;
		GameMgr.GoblinArmyEnabled( true );
		GameMgr.PlaceGoblinArmy( true );
	}

	public override void Changed( GameState state )
	{
		hud?.Delete( true );
		
		if ( Game.IsServer )
		{
			foreach ( var entity in Entity.All.OfType<BaseStructure>() )
				entity.Delete();
		}
	}

	public static bool TrySpawnStructure( BuildingEntry entry, out BaseStructure structure )
	{
		structure = null;

		if ( !Structures.Contains( entry ) )
		{
			Structures.Add( entry );
			Log.Error( "Registered structure." );
		}

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
