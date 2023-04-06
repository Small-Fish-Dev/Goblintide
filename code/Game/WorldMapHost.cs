namespace GameJam;

public partial class WorldMapHost : HostEntity<WorldMapHost>
{
	public static IEnumerable<IEntry> Entries => All.OfType<IEntry>();
	public static bool IsEmpty { get; set; } = true;
	public static float MapSize { get; set; } = 5000f;

	public interface IEntry
	{
		public Vector2 MapPosition { get; }
	}

	public partial class Generator : Entity, IEntry
	{
		[Net] public double Size { get; set; }
		[Net] public Vector2 MapPosition { get; set; }

		public Generator() { }

		public Generator( Vector2 position, double size ) : this()
		{
			IsEmpty = false;
			Transmit = TransmitType.Always;
			if ( !Game.IsServer ) return;
			MapPosition = position;
			Size = size;
		}
	}

	public static void GenerateNew()
	{
		int maxChecks = 15;
		var gaps = MapSize / (maxChecks * 2);
		for ( int x = -maxChecks; x < maxChecks; x++ )
		{
			for ( int y = -maxChecks; y < maxChecks; y++ )
			{
				if ( x == 0 && y == 0 ) continue;
				if ( Game.Random.Int( 3 ) == 0 ) continue;

				var randomOffset = Vector2.Random.Normal * gaps / 4f;
				var position = new Vector2( x, y ) * gaps + randomOffset;
				var distanceFromCenter = position.Distance( Vector2.Zero );
				new Generator( position, Math.Pow( distanceFromCenter / 40f, 1.4f ) );
			}
		}
	}

	[ConCmd.Server]
	private static void ClientToServerGenerate( int idx )
	{
		Game.AssertServer();
		var entity = FindByIndex( idx );
		if ( entity is not Generator generator ) return;

		var energyRequired = (int)( generator.Size / 2f );

		if ( GameMgr.TotalEnergy >= energyRequired )
		{
			Town.GenerateTown( (float)generator.Size );
			GameMgr.SetState<RaidingState>();
			GameMgr.TotalEnergy -= energyRequired;
		}
	}

	public static void RequestServerGenerate( Generator generator )
	{
		Game.AssertClient();
		ClientToServerGenerate( generator.NetworkIdent );
	}
}
