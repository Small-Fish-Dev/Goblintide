namespace GameJam;

partial class GameMgr
{
	/// <summary>
	/// The current GameState.
	/// </summary>
	public static GameState State => Instance.state;
	[Net, Change( "gameStateChanged" )] private GameState state { get; set; }

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

		if ( State != null )
			Event.Unregister( State );

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
		current?.Initialize();
	}

	[ConCmd.Admin( "startraid" )]
	public static void StartRaid( double size )
	{
		var energyRequired = (int)(size / 2f);

		if ( GameMgr.TotalEnergy >= energyRequired )
		{
			Town.GenerateTown( (float)size );
			GameMgr.SetState<RaidingState>();
			GameMgr.TotalEnergy -= energyRequired;
		}
	}
}
