﻿namespace GoblinGame;

partial class Lord
{
	[ClientRpc]
	public static void _addEventlog( string text, float time )
	{
		EventLogger.Instance?.Append( text, time );
	}
	[ClientRpc]
	public static void _addGameplayHint( string text, float time )
	{
		GameplayHints.Instance?.Append( text, time );
	}

	[ConCmd.Server( "eventlog" )]
	public static void _logCommand( string input, float time = 5f )
	{
		if ( ConsoleSystem.Caller.Pawn is not Lord pawn )
			return;

		Lord._addEventlog( To.Single( pawn ), input, time );
	}
}
