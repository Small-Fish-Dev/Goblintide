using GameJam.UI;

namespace GameJam;

public static class MenuControls
{
	[Event.Client.BuildInput]
	private static void BuildInput()
	{
		if ( GameMgr.State is VillageState)
		{
			if ( Input.Pressed( InputButton.Reload ) ) WorldMap.Toggle();
			if ( Input.Pressed( InputButton.Chat ) ) SkillTree.Toggle();
		}
	}
}
