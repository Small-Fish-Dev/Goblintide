global using Sandbox;
global using System;
global using Editor;
global using System.Collections.Generic;
global using System.Linq;

namespace GameJam;

// TODO(gio): rename GameMgr when we have an actual idea! 
public partial class GameMgr : GameManager
{
	/// <summary>
	/// The singleton for GameManager.
	/// </summary>
	public static GameMgr Instance { get; private set; }

	/// <summary>
	/// The current GameState.
	/// </summary>
	public static GameState State => Instance.state;
	[Net, Change( "gameStateChanged" )] private GameState state { get; set; }

	public GameMgr()
	{
		Instance = this;
		Game.TickRate = 30;

		// Set the beginning state.
		if ( Game.IsServer )
			SetState<VillageState>();

		Event.Run( nameof( GameEvents.Initialize ) );
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

	/// <summary>
	/// Set the current GameState to T : GameState.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns>The new state if setting it was successful.</returns>
	public static T SetState<T>() where T : GameState
	{
		// We shouldn't be setting the state on client.
		Game.AssertServer();

		var type = typeof( T );
		if ( State != null && type == State?.GetType() )
		{
			Log.Error( "We don't want to set the same GameState twice." );
			return null;
		}

		// Handle creating new state.
		var newState = (T)TypeLibrary.Create( type.FullName, type );
		if ( newState == null )
			return null;

		State?.Changed( newState );
		newState.Initialize();

		// Override the old state.
		Instance.state = newState;
		return newState;
	}

	// Handle changing states on client.
	private static void gameStateChanged( GameState old, GameState current )
	{
		// Call changed.
		old?.Changed( current );

		// Call initialize.
		current.Initialize();
	}
}
