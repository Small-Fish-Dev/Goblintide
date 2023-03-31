global using Sandbox;
global using System;
global using Editor;
global using System.Collections.Generic;
global using System.Linq;
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

	public GameMgr()
	{
		Instance = this;
		Game.TickRate = 15;

		// Set the beginning state.
		if ( Game.IsServer )
			SetState<VillageState>();

		Event.Run( nameof( GameEvents.Initialize ) );
		
		WorldMapHost.Start();
		
		UpgradeInstanceCreator.RepopulateKnownUpgrades();
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var pawn = new Lord();
		client.Pawn = pawn;

		if ( All.OfType<SpawnPoint>().MinBy( x => Guid.NewGuid() ) is not { } spawn )
			return;

		var transform = spawn.Transform;
		transform.Position += Vector3.Up * 50.0f;
		pawn.Transform = transform;
	}
}
