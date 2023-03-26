﻿global using Sandbox;
global using System;
global using Editor;
global using System.Collections.Generic;
global using System.Linq;

namespace GameJam;

// TODO(gio): rename GameMgr when we have an actual idea! 
public partial class GameMgr : GameManager
{
	public GameMgr()
	{
		Game.TickRate = 30;
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
