using GoblinGame.UI;

namespace GoblinGame;

public static class MenuControls
{
	[Event.Client.BuildInput]
	private static void BuildInput()
	{
		if ( Goblintide.State is not VillageState )
			return;

		if ( Input.Pressed( InputButton.Reload ) )
		{
			WorldMap.Toggle();
			closeOverview();
		}
		else if ( Input.Pressed( InputButton.Chat ) )
		{
			SkillTree.Toggle();
			closeOverview();
		}
	}

	[ConCmd.Server]
	private static void closeOverview()
	{
		if ( ConsoleSystem.Caller.Pawn is not Lord pawn )
			return;

		pawn.Overview = false;
	}
}
