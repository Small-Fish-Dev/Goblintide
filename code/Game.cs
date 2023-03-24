global using Sandbox;
global using System;
global using System.Collections.Generic;
global using System.Linq;

namespace GameJam;

public partial class GameMgr : GameManager
{
	public GameMgr()
	{
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var pawn = new Pawn();
		client.Pawn = pawn;

		if ( All.OfType<SpawnPoint>().MinBy( x => Guid.NewGuid() ) is not { } spawn )
			return;

		var transform = spawn.Transform;
		transform.Position += Vector3.Up * 50.0f;
		pawn.Transform = transform;
	}
}
