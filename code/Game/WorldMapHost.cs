namespace GameJam;

public partial class WorldMapHost : HostEntity<WorldMapHost>
{
	public static IEnumerable<IEntry> Entries => All.OfType<IEntry>();

	public interface IEntry
	{
		public Vector2 MapPosition { get; }
	}

	public partial class Generator : Entity, IEntry
	{
		[Net] public float Size { get; set; }
		[Net] public Vector2 MapPosition { get; set; }

		public Generator( Vector2 position, float size )
		{
			Transmit = TransmitType.Always;
			if ( !Game.IsServer ) return;
			MapPosition = position;
			Size = size;
		}
	}

	[ConCmd.Server]
	private static void ClientToServerGenerate( int idx )
	{
		Game.AssertServer();
		var entity = FindByIndex( idx );
		if ( entity is not Generator generator ) return;
		Town.GenerateTown( ConsoleSystem.Caller.Pawn.Position, generator.Size );
	}

	public static void RequestServerGenerate( Generator generator )
	{
		Game.AssertClient();
		ClientToServerGenerate( generator.NetworkIdent );
	}
}
