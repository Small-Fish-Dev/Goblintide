using System.IO;
using System.IO.Compression;
using System.Text.Json;
using Graphicator.Formats;

namespace Graphicator.Internal;

public interface IAnimationEntry
{
}

public struct AnimationFrame : IAnimationEntry
{
	public Texture Texture { get; set; }
}

public struct AnimationControlFrame : IAnimationEntry
{
	public float? NewDelay;
}

/// <summary> Animation loaded from the AnimationPack format </summary>
public class Animation : IDisposable
{
	private static readonly List<(string, Animation)> Instances = new();
	public string Path { get; private set; }
	public List<IAnimationEntry> Entries { get; } = new();
	private bool _disposed;

	public Animation( string path )
	{
		Path = path;
		Event.Register( this );
	}

	#region Instance Swapping

	private const float ChecksumInterval = 2.0f;
	private const float SizeInterval = 0.2f;
	private bool _checkingCrc;
	private bool _checkingSize;
	private TimeUntil _nextCrc = 2f;
	private TimeUntil _nextSize = 0.2f;
	private long? _lastSize;
	private ulong? _lastCrc;
	private bool _swapPrepared;
	private TimeUntil _nextSwapInvokeCheck;

	internal class InstanceSwapEventArgs : EventArgs
	{
		public Animation Next;
	}

	internal event EventHandler<InstanceSwapEventArgs> ProcessingInstanceSwap;

	private void InvokeSwap()
	{
		if ( _disposed ) return;

		Log.Info( $"Swapping {Path}" );

		Instances.RemoveAll( v => v.Item1 == Path );

		var next = Load( Path );

		ProcessingInstanceSwap?.Invoke( this, new InstanceSwapEventArgs { Next = next } );

		Dispose();
	}

	private void PrepareSwap()
	{
		_swapPrepared = true;
		_nextSwapInvokeCheck = 0f;
	}

	private async void CheckCrc()
	{
		if ( _disposed ) return;
		if ( Path == null ) return;
		_checkingCrc = true;
		var crc = await FileSystem.Mounted.GetCRC( Path );
		if ( crc != _lastCrc && _lastCrc != null )
			PrepareSwap();
		_lastCrc = crc;
		_checkingCrc = false;
	}

	private void CheckSize()
	{
		if ( _disposed ) return;
		if ( Path == null ) return;
		_checkingSize = true;
		var size = FileSystem.Mounted.FileSize( Path );
		if ( size != _lastSize && _lastSize != null )
			PrepareSwap();
		_lastSize = size;
		_checkingSize = false;
	}

	/// <summary>
	/// Called every frame - we check every once in a while if the file was updated
	/// </summary>
	[Event.Client.Frame]
	private void Frame()
	{
		if ( _swapPrepared && _nextSwapInvokeCheck )
		{
			_nextSwapInvokeCheck = 0.2f;
			try
			{
				var f = FileSystem.Mounted.OpenRead( Path );
				f.Close();
			}
			catch ( Exception )
			{
				// If we couldn't open the file, give up!
				return;
			}

			InvokeSwap();
			return;
		}

		if ( _checkingCrc || _checkingSize ) return;

		if ( _nextCrc )
		{
			_nextCrc = ChecksumInterval;
			GameTask.RunInThreadAsync( CheckCrc );
			return;
		}

		if ( !_nextSize ) return;
		_nextSize = SizeInterval;
		GameTask.RunInThreadAsync( CheckSize );
	}

	#endregion

	private static void HandleCtrlEntry( Animation animation, ZipArchive archive, string right )
	{
		var f = archive.GetEntry( $"ctrl/{right}" );
		if ( f == null )
			throw new AnimationPackException( $"Ctrl entry {right} not found" );
		using var entryStream = f.Open();
		var ctrl = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>( entryStream );
		var frame = new AnimationControlFrame();
		if ( ctrl.TryGetValue( "core.delay", out var v ) )
		{
			frame.NewDelay = v.GetSingle();
		}

		animation.Entries.Add( frame );
	}

	private static void HandleFrameEntry( Animation animation, ZipArchive archive, string right )
	{
		var f = archive.GetEntry( $"frame/{right}" );
		if ( f == null )
			throw new AnimationPackException( $"Frame {right} not found" );
		using var stream = f.Open();
		var img = Qoi.Read( stream, f.Length );
		var frame = new AnimationFrame { Texture = img.CreateGameTexture() };
		animation.Entries.Add( frame );
	}

	public static Animation Find( string path )
	{
		foreach ( var (existingPath, animation) in Instances )
		{
			if ( existingPath == path )
				return animation;
		}

		return Load( path );
	}

	private static Animation Load( string path )
	{
		var result = new Animation( path );

		using var stream = FileSystem.Mounted.OpenRead( path );
		using var archive = new ZipArchive( stream, ZipArchiveMode.Read );

		// Read the manifest
		AnimationPack.Manifest manifest;
		try
		{
			var f = archive.GetEntry( "manifest.json" );

			if ( f == null )
				throw new AnimationPackException( "Manifest not found" );

			using var entryStream = f.Open();
			manifest = JsonSerializer.Deserialize<AnimationPack.Manifest>( entryStream );
			Log.Info( $"Found {manifest}" );
		}
		catch ( Exception e )
		{
			throw new AnimationPackException( "Failed to deserialize manifest", e );
		}

		// Read entries
		foreach ( var entry in manifest.Entries )
		{
			var split = entry.Split( ":" );
			if ( split.Length != 2 )
				throw new AnimationPackException( $"Unknown entry \"{entry}\"" );
			var left = split[0];
			var right = split[1];
			switch ( left )
			{
				case "ctrl":
					// Handle control entry
					HandleCtrlEntry( result, archive, right );
					break;
				case "frame":
					// Handle frame
					HandleFrameEntry( result, archive, right );
					break;
				default:
					throw new AnimationPackException( $"Unknown left side of \"{entry}\"" );
			}
		}

		Instances.Add( (path, result) );
		return result;
	}

	public void Dispose()
	{
		Event.Unregister( this );
		foreach ( var frame in Entries.OfType<AnimationFrame>() ) frame.Texture?.Dispose();
		Entries.Clear();
		_disposed = true;
	}
}

public class AnimationPackException : Exception
{
	public AnimationPackException() { }

	public AnimationPackException( string message )
		: base( $"AnimationPack corrupt or incomplete: {message}" )
	{
	}

	public AnimationPackException( string message, Exception inner )
		: base( $"AnimationPack corrupt or incomplete: {message}", inner )
	{
	}
}
