namespace GameJam;

public static class Debug
{
	[ConVar.Client( "gdbg_camera" )] private static bool ShowCameraInfo { get; set; } = true;
	[ConVar.Client( "gdbg_player" )] private static bool ShowPlayerInfo { get; set; } = true;
	[ConVar.Client( "gdbg_lord" )] private static bool ShowLordInfo { get; set; } = true;
	[ConVar.Client( "gdbg_input" )] private static bool ShowInputInfo { get; set; } = true;

	private static Vector3 _analogMove;
	private static Angles _analogLook;

	[Event.Client.BuildInput]
	private static void BuildInput()
	{
		_analogMove = Input.AnalogMove;
		_analogLook = Input.AnalogLook;
	}
	
	[Event.Client.Frame]
	private static void Frame()
	{
		var pos = Vector2.One * 20;
		var line = 0;

		#region Methods

		void Add( string text, Color color ) => DebugOverlay.ScreenText( text, pos, line++, color );

		void AddDual( string text, Color color, string text2, Color color2 )
		{
			DebugOverlay.ScreenText( text, pos, line, color );
			DebugOverlay.ScreenText( $"{new string( ' ', text.Length )}{text2}", pos, line, color2 );
			line++;
		}

		void Header( string name ) => Add( name, Color.Yellow );

		void Section( string name, Action action, bool active = true )
		{
			if ( !active ) return;
			Header( name );
			action.Invoke();
			Space();
		}

		void Value( string name, string value ) => AddDual( $"{name}: ", Color.Orange, value, Color.White );
		void ValueObject( string name, object value ) => Value( name, value.ToString() );
		void Space( int amount = 1 ) => line += amount;

		#endregion

		AddDual( "Small Fish Confidential - ", Color.Yellow, "Unauthorised distribution will end in death",
			Color.Yellow );
		Add( DateTime.Now.ToString(), Color.White );

		Space();

		// Player info
		Section( "Game Info", () =>
		{
			var pawn = Game.LocalPawn;
			ValueObject( "Position", pawn.Position );
			ValueObject( "Rotation", pawn.Rotation );
			ValueObject( "Steam ID", Game.LocalClient.SteamId );
			ValueObject( "Entity Count", Entity.All.Count );
		}, ShowPlayerInfo );

		Section( "Input", () =>
		{
			ValueObject( "AnalogMove", _analogMove );
			ValueObject( "AnalogLook", _analogLook );
		}, ShowInputInfo );
		
		Section( "Lord", () =>
		{
			var lord = (Lord)Game.LocalPawn;
			ValueObject( "Faction", lord.Faction );
			ValueObject( "Hit Points", lord.HitPoints );
		}, ShowLordInfo );

		Section( "Camera", () =>
		{
			var lord = (Lord)Game.LocalPawn;
			ValueObject( "Position", Camera.Position );
			ValueObject( "Rotation", Camera.Rotation );
			ValueObject( "Interim", lord.InterimCameraRotation );
		}, ShowCameraInfo );
		
	}
}
