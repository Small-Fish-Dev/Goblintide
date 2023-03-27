namespace GameJam;

public partial class VillageState : GameState
{
	TimeSince lastTicked;

	public override void Initialize()
	{
	}

	[Event.Tick]
	private void onTick()
	{
		if ( lastTicked < 2f )
			return;

		lastTicked = 0f;
	}
}
