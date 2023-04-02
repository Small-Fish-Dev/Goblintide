global using Sandbox;
global using System;
global using Editor;
global using System.Collections.Generic;
global using System.Linq;
global using Sandbox.Utility;
global using System.Threading.Tasks;
global using Sandbox.Component;
using GameJam.UpgradeSystem;

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
	[Net] private Lord lord { get; set; }

	public GameMgr()
	{
		Instance = this;
		Game.TickRate = 15;

		Event.Run( nameof(GameEvents.Initialize) );

		WorldMapHost.Start();
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
