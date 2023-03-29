using GameJam.UI;

namespace GameJam;

public static class MenuControls
{
	[Event.Client.BuildInput]
	private static void BuildInput()
	{
		if ( Input.Pressed( InputButton.Reload ) ) WorldMap.Toggle();
	}
}
