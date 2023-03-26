namespace GameJam;

public partial class VillageState : GameState
{
	TimeSince lastTicked;

	public override void Initialize()
	{
		Log.Error( $"Hey! We are on {(Game.IsClient ? "client" : "server")}." );
	}

	[Event.Tick]
	private void onTick()
	{
		if ( lastTicked < 2f )
			return;

		Log.Error( $"TICK: Hey! We are on {(Game.IsClient ? "client" : "server")}." );
		lastTicked = 0f;
	}
}
