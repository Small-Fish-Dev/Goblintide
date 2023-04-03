global using Sandbox;
global using System;
global using Editor;
global using System.Collections.Generic;
global using System.Linq;
global using Sandbox.Utility;
global using System.Threading.Tasks;
global using Sandbox.Component;
using GameJam.UpgradeSystem;
using GameJam.Props.Collectable;

namespace GameJam;

// TODO(gio): rename GameMgr when we have an actual idea! 
public partial class GameMgr : GameManager
{
	/// <summary>
	/// The singleton for GameManager.
	/// </summary>
	public static GameMgr Instance { get; private set; }

	/// <summary>
	/// The game's current Lord.
	/// </summary>
	public static Lord Lord
	{
		get => Instance.lord;
		set
		{
			Instance.lord = value;
		}
	}
	/// <summary>
	 /// The game's current Town.
	 /// </summary>
	public static Town CurrentTown
	{
		get => Instance.currentTown;
		set
		{
			Instance.currentTown = value;
		}
	}
	/// <summary>
	 /// The game's current Town.
	 /// </summary>
	public static IList<BaseNPC> GoblinArmy
	{
		get => Instance.goblinArmy;
		set
		{
			Instance.goblinArmy = value;
		}
	}
	[Net] private Lord lord { get; set; }
	[Net] private Town currentTown { get; set; }
	[Net] private IList<BaseNPC> goblinArmy { get; set; }

	public static int TotalGold
	{
		get => Instance.totalGold;
		set
		{
			Instance.totalGold = value;
		}
	}
	public static int TotalWood
	{
		get => Instance.totalWood;
		set
		{
			Instance.totalWood = value;
		}
	}
	public static int TotalFood
	{
		get => Instance.totalFood;
		set
		{
			Instance.totalFood = value;
		}
	}
	public static int TotalWomen
	{
		get => Instance.totalWomen;
		set
		{
			Instance.totalWomen = value;
		}
	}
	[Net] private int totalWood { get; set; }
	[Net] private int totalGold { get; set; }
	[Net] private int totalFood { get; set; }
	[Net] private int totalWomen { get; set; }

	public GameMgr()
	{
		Instance = this;
		Game.TickRate = 15;

		Event.Run( nameof(GameEvents.Initialize) );

		WorldMapHost.Start();
	}

	public static void AddResource( Collectable type, int amount )
	{
		if ( type == Collectable.Wood )
			TotalWood += amount;
		if ( type == Collectable.Gold )
			TotalGold += amount;
		if ( type == Collectable.Food )
			TotalFood += amount;
		if ( type == Collectable.Woman )
			TotalWomen += amount;
	}

	public override void ClientJoined( IClient client )
	{
		// Hard code player limit to 1.
		if ( Lord != null )
		{
			client.Kick();
			return;
		}

		base.ClientJoined( client );

		var pawn = new Lord();
		client.Pawn = pawn;
		Lord = pawn;

		if ( All.OfType<SpawnPoint>().MinBy( x => Guid.NewGuid() ) is { } spawn )
		{
			var transform = spawn.Transform;
			transform.Position += Vector3.Up * 50.0f;
			pawn.Transform = transform;
		}

		// Load the save.
		SetState<VillageState>();
		LoadSave();
	}

	[ClientRpc]
	public static void BroadcastTrees( Vector3 position, float townWidth )
	{
		Town.PlaceTrees( position, townWidth );
	}

	[ClientRpc]
	public static void BroadcastFences( Vector3 position, float townWidth )
	{
		Town.PlaceFences( position, townWidth );
	}
}
