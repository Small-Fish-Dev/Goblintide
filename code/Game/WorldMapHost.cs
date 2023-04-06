using System.IO;

namespace GameJam;

public partial class WorldMapHost : HostEntity<WorldMapHost>
{
	public static IReadOnlyList<Node> Entries => entries;
	public static bool IsEmpty { get; set; } = true;
	public static float MapSize { get; set; } = 5000f;

	private static List<Node> entries = new();

	public class Node
	{
		public double Size { get; set; }
		public Vector2 MapPosition { get; set; }
		public int Index { get; }

		public Node( Vector2 position, double size, int? id = null )
		{
			IsEmpty = false;
			MapPosition = position;
			Size = size;
			Index = id ?? entries.Count;

			entries.Insert( Index, this );
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
				var node = new Node( position, Math.Pow( distanceFromCenter / 40f, 1.4f ) );
			}
		}
	}

	public static void BroadcastNodes( To target = default )
	{
		Game.AssertServer();

		using var stream = new MemoryStream();
		using var writer = new BinaryWriter( stream );

		writer.Write( entries.Count );
		foreach ( var node in entries )
		{
			writer.Write( node.Index );
			writer.Write( node.MapPosition );
			writer.Write( node.Size );
		}

		SendNodeData( target, stream.ToArray() );
	}

	[ClientRpc]
	public static void SendNodeData( byte[] data )
	{
		using var stream = new MemoryStream( data );
		using var reader = new BinaryReader( stream );

		var count = reader.ReadInt32();
		for ( int i = 0; i < count; i++ )
		{
			var index = reader.ReadInt32();
			var position = reader.ReadVector2();
			var size = reader.ReadDouble();

			var node = new Node( position, size, index );
		}
	}

	[ConCmd.Server]
	private static void ClientToServerGenerate( int index )
	{
		Game.AssertServer();

		// Failed to find node.
		Node? node;
		if ( (node = Entries.ElementAtOrDefault( index )) == null )
			return;

		var energyRequired = (int)( node.Size / 2f );

		if ( GameMgr.TotalEnergy >= energyRequired )
		{
			Town.GenerateTown( (float)node.Size );
			GameMgr.SetState<RaidingState>();
			GameMgr.TotalEnergy -= energyRequired;
		}
	}

	public static void RequestServerGenerate( Node generator )
	{
		Game.AssertClient();
		ClientToServerGenerate( generator.Index );
	}
}
