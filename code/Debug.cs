using System.Globalization;
using System.Text;

namespace GameJam;

public static class Debug
{
	private const string DrawEventName = "gdbg_event";
	private const string PreDrawEventName = "gdbg_preevent";

	public class DrawAttribute : EventAttribute
	{
		public DrawAttribute() : base( DrawEventName ) { }
	}

	public class PreDrawAttribute : EventAttribute
	{
		public PreDrawAttribute() : base( PreDrawEventName ) { }
	}

	[ConVar.Client( "gdbg" )] private static bool Enabled { get; set; } = true;
	[ConVar.Client( "gdbg_player" )] private static bool ShowPlayerInfo { get; set; } = true;
	[ConVar.Client( "gdbg_lord" )] private static bool ShowLordInfo { get; set; } = true;
	[ConVar.Client( "gdbg_input" )] private static bool ShowInputInfo { get; set; } = true;
	[ConVar.Client( "gdbg_git" )] private static bool ShowGitInfo { get; set; } = true;

	[ConVar.Client( "gdbg_ext_npc" )] private static bool ShowExtendedNpcData { get; set; } = true;

	[ConVar.Client( "gdbg_git_shorttags" )]
	private static bool ShortenTags { get; set; } = true;

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
		}
		catch ( Exception e )
		{
			// ignored
			Log.Warning( e );
		}

		Space();
	}

	internal static void Value( string name, string value ) => Add( $"{name}: ", Color.Orange, value, Color.White );
	internal static void Value( string name, object value ) => Value( name, value.ToString() );
	internal static void Space( int amount = 1 ) => _line += amount;

	#endregion

	[Event.Client.Frame]
	private static void Frame()
	{
		if ( !Enabled ) return;

		Event.Run( PreDrawEventName );

		_line = 0;

		TryGitUpdate();

		Add( "Small Fish Confidential - ", Color.Yellow, "Unauthorised distribution may result in death",
			Color.Yellow );
		Add( DateTime.Now.ToString( CultureInfo.InvariantCulture ), Color.White );

		Space();

		SetHeaderColor( Color.Yellow );

		if ( _branches.Count != 0 )
			Section( "Commit", () =>
			{
				foreach ( var (branch, id, src) in _branches ) Value( $"{branch} ({src})", ShortenTags ? id[..7] : id );
			}, ShowGitInfo );

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
			Value( "Energy", lord.Energy.Value );
		}, ShowLordInfo );

		SetHeaderColor( Color.Green );

		Event.Run( DrawEventName );
	}

	#region Git

	private static readonly List<(string, string, string)> _branches = new();
	private static TimeUntil _gitUpdateTimer = 5;

	private static void TryGitUpdate()
	{
		if ( !_gitUpdateTimer )
			return;

		GitUpdate();
		_gitUpdateTimer = 5;
	}

	[Event.Hotload, GameEvents.Initialize]
	private static void GitUpdate()
	{
		_branches.Clear();

		try
		{
			var git = FileSystem.Mounted.CreateSubSystem( ".git" );
			{
				var contents = git.ReadAllText( "refs/heads/main" );
				_branches.Add( ("main", contents, "local") );
			}
			if ( git.FileExists( "FETCH_HEAD" ) )
			{
				var contents = git.ReadAllText( "FETCH_HEAD" );
				_branches.Add( ("main", contents.Split( "\t" )[0], "fetched") );
			}
		}
		catch ( Exception )
		{
			// ignored
		}
	}

	#endregion

	#region NPC Info

	[PreDraw]
	private static void DrawNpcInfo()
	{
		if ( !ShowExtendedNpcData ) return;
		foreach ( var v in Entity.All.OfType<BaseNPC>() )
		{
			var pos = v.Position;
			var line = 0;
			DebugOverlay.Line( pos, pos + v.Direction * 32 );
			DebugOverlay.Text( $"<3 {v.HitPoints}", pos, line++, Color.White );
			DebugOverlay.Text( $"!  {v.CurrentBehaviour}", pos, line++, Color.White );

			if ( v.CurrentTarget != null )
				DebugOverlay.Text( $"-> {v.CurrentTarget}", pos, line++, Color.White );
		}
	}

	#endregion
}
