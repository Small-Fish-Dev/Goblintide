using System.Globalization;

namespace GameJam;

public static class Debug
{
	private const string DrawEventName = "gdbg_event";

	public class DrawAttribute : EventAttribute
	{
		public DrawAttribute() : base( DrawEventName ) { }
	}

	[ConVar.Client( "gdbg_camera" )] private static bool ShowCameraInfo { get; set; } = true;
	[ConVar.Client( "gdbg_player" )] private static bool ShowPlayerInfo { get; set; } = true;
	[ConVar.Client( "gdbg_lord" )] private static bool ShowLordInfo { get; set; } = true;
	[ConVar.Client( "gdbg_input" )] private static bool ShowInputInfo { get; set; } = true;

	#region Input Processing

	private static Vector3 _analogMove;
	private static Angles _analogLook;

	[Event.Client.BuildInput]
	private static void BuildInput()
	{
		_analogMove = Input.AnalogMove;
		_analogLook = Input.AnalogLook;
	}

	#endregion

	#region Overlay Methods

	private static int _line;
	private static readonly Vector2 Pos = Vector2.One * 20;
	private static Color _headerColor = Color.Yellow;

	private static void SetHeaderColor( Color color ) => _headerColor = color;

	internal static void Add( string text, Color color ) => DebugOverlay.ScreenText( text, Pos, _line++, color );
	internal static void Add( string text ) => DebugOverlay.ScreenText( text, Pos, _line++, Color.White );

	internal static void Add( string text, Color color, string text2, Color color2 )
	{
		DebugOverlay.ScreenText( text, Pos, _line, color );
		DebugOverlay.ScreenText( $"{new string( ' ', text.Length )}{text2}", Pos, _line, color2 );
		_line++;
	}

	internal static void Header( string name ) => Add( name, _headerColor );

	internal static void Section( string name, Action action, bool active = true )
	{
		if ( !active ) return;
		try
		{
			Header( name );
			action.Invoke();
			Space();
		}
		catch ( Exception )
		{
			// ignored
		}
	}

	internal static void Value( string name, string value ) => Add( $"{name}: ", Color.Orange, value, Color.White );
	internal static void Value( string name, object value ) => Value( name, value.ToString() );
	internal static void Space( int amount = 1 ) => _line += amount;

	#endregion

	[Event.Client.Frame]
	private static void Frame()
	{
		_line = 0;

		Add( "Small Fish Confidential - ", Color.Yellow, "Unauthorised distribution will end in death",
			Color.Yellow );
		Add( DateTime.Now.ToString( CultureInfo.InvariantCulture ), Color.White );

		Space();

		SetHeaderColor( Color.Yellow );

		// Player info
		Section( "Game Info", () =>
		{
			var pawn = Game.LocalPawn;
			Value( "Position", pawn.Position );
			Value( "Rotation", pawn.Rotation );
			Value( "Steam ID", Game.LocalClient.SteamId );
			Value( "Entity Count", Entity.All.Count );
		}, ShowPlayerInfo );

		Section( "Input", () =>
		{
			Value( "AnalogMove", _analogMove );
			Value( "AnalogLook", _analogLook );
		}, ShowInputInfo );

		Section( "Lord", () =>
		{
			var lord = (Lord)Game.LocalPawn;
			Value( "Faction", lord.Faction );
			Value( "Hit Points", lord.HitPoints );
		}, ShowLordInfo );

		Section( "Camera", () =>
		{
			var lord = (Lord)Game.LocalPawn;
			Value( "Position", Camera.Position );
			Value( "Rotation", Camera.Rotation );
			Value( "Interim", lord.InterimCameraRotation );
		}, ShowCameraInfo );

		SetHeaderColor( Color.Green );

		Event.Run( DrawEventName );
	}
}
