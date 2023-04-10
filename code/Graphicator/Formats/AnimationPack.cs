using System;
using System.Collections.Generic;

namespace Graphicator.Formats;

/**
 * 1: Format Header[4] {
 *	Magic[3] = "ANM"
 *	Version[1] = ?
 * }
 *
 * 2: Animation Header[8] {
 *	Frames Per Second[4] = ?
 *  Part Count[4] = ?
 * }
 *
 * (abstract) Part[?] {
 *	Part Type [1] = ?
 *  Content Size [4] = ? // Doesn't include header size 
 * }
 *
 * ?: Image Part[?] : Part {
 *	Part Type [1] = 1
 *  Content Size [4] = ?
 *  Content [?] = ?
 * }
 *
 * ?: Set Delay Part[9] : Part {
 *	Part Type [1] = 2
 *	Content Size [4] = 4
 *  Content [4] = ?
 * }
 */
public struct Reader
{
	private readonly byte[] _content;
	public int Index { get; private set; }

	public void Skip( int num ) => Index += num;

	public byte Byte()
	{
		var v = _content[Index];
		Index++;
		return v;
	}

	public byte Peek() => _content[Index];

	public int Int32()
	{
		var v = BitConverter.ToInt32( _content, Index );
		Index += 4;
		return v;
	}

	public int BeInt32() =>
		(_content[Index++] << 24) | (_content[Index++] << 16)
		                          | (_content[Index++] << 8) | _content[Index++];

	public uint BeUint32() =>
		(uint)((_content[Index++] << 24) | (_content[Index++] << 16)
		                                 | (_content[Index++] << 8) | _content[Index++]);

	public float Float()
	{
		var v = BitConverter.ToSingle( _content, Index );
		Index += 4;
		return v;
	}

	public Reader( byte[] content ) => _content = content;
}

public class AnimationPack
{
	public enum PartType
	{
		Unknown = 0,
		Image = 1,
		SetDelayFloat = 2,
	}

	public int FramesPerSecond { get; private set; }
	public int PartCount { get; private set; }
	public byte FileVersion { get; private set; }
	public List<IPart> Parts { get; } = new();

	public const byte MinVersion = 0;
	public const byte MaxVersion = 1;

	public interface IPart
	{
		public PartType Type { get; }
	}

	public struct ImagePart : IPart
	{
		public enum QoiChannels
		{
			Rgb = 3,
			Rgba = 4
		}

		public enum QoiColorspace
		{
			LinearAlphaSrgb = 0,
			Linear = 1
		}

		public enum QoiOp
		{
			Index = 0x00,
			Diff = 0x40,
			Luma = 0x80,
			Run = 0xc0,
			Rgb = 0xfe,
			Rgba = 0xff
		}

		public const byte QoiMask2 = 0xc0;

		public static int QoiColorHash( byte[] pixel ) => pixel[0] * 3 + pixel[1] * 5 + pixel[2] * 7 + pixel[3] * 11;

		public PartType Type { get; }
		public byte[] Content { get; private set; }
		public uint Width;
		public uint Height;
		public QoiChannels Channels;
		public QoiColorspace Colorspace;

		public Texture CreateTexture() =>
			Texture.Create( (int)Width, (int)Height )
				.WithData( Content )
				.WithFormat( ImageFormat.BGRA8888_LINEAR )
				.Finish();

		public static ImagePart Read( ref Reader reader, AnimationPack animationPack )
		{
			var size = reader.Int32(); // Get content size
			var chunkContentSize = size - 8;
			var chunkContentEnd = reader.Index + chunkContentSize;

			// Get our image part ready
			var part = new ImagePart();

			// Read QOI magic
			if ( !(reader.Byte() == 'q' && reader.Byte() == 'o' && reader.Byte() == 'i' && reader.Byte() == 'f') )
				throw new AnimationPackException(
					$"QOI image magic not found (@ {reader.Index}, found {reader.Peek()})" );

			part.Width = reader.BeUint32();
			part.Height = reader.BeUint32();
			part.Channels = (QoiChannels)reader.Byte();
			part.Colorspace = (QoiColorspace)reader.Byte();

			part.Content = new byte[part.Width * part.Height * (byte)part.Channels];

			// Iteration variables
			var index = new byte[64 * 4]; // Previously seen pixel value array
			var px = new byte[] { 0, 0, 0, 255 };
			var run = 0;

			Log.Info( part.Channels );
			Log.Info( part.Colorspace );
			Log.Info( part.Content.Length );

			for ( var pos = 0; pos < part.Content.Length; pos += (byte)part.Channels )
			{
				if ( run > 0 )
				{
					run--;
				}
				else if ( reader.Index < chunkContentEnd )
				{
					var b1 = reader.Byte();

					switch ( b1 )
					{
						case (byte)QoiOp.Rgb:
							px[0] = reader.Byte();
							px[1] = reader.Byte();
							px[2] = reader.Byte();
							break;
						case (byte)QoiOp.Rgba:
							px[0] = reader.Byte();
							px[1] = reader.Byte();
							px[2] = reader.Byte();
							px[3] = reader.Byte();
							break;
						default:
							switch ( b1 & QoiMask2 )
							{
								case (byte)QoiOp.Index:
									var idx4 = (b1 & ~QoiMask2) * 4;
									px[0] = index[idx4];
									px[1] = index[idx4 + 1];
									px[2] = index[idx4 + 2];
									px[3] = index[idx4 + 3];
									break;
								case (byte)QoiOp.Diff:
									px[0] += (byte)(((b1 >> 4) & 0x03) - 2);
									px[1] += (byte)(((b1 >> 2) & 0x03) - 2);
									px[2] += (byte)((b1 & 0x03) - 2);
									break;
								case (byte)QoiOp.Luma:
									var b2 = reader.Byte();
									var vg = (b1 & 0x3f) - 32;
									px[0] += (byte)(vg - 8 + ((b2 >> 4) & 0x0F));
									px[1] += (byte)vg;
									px[2] += (byte)(vg - 8 + (b2 & 0x0F));
									break;
								case (byte)QoiOp.Run:
									run = b1 & 0x3f;
									break;
							}

							break;
					}

					{
						var idx4 = QoiColorHash( px ) % 64 * 4;
						index[idx4] = px[0];
						index[idx4 + 1] = px[1];
						index[idx4 + 2] = px[2];
						index[idx4 + 3] = px[3];
					}
				}

				part.Content[pos + 0] = px[0];
				part.Content[pos + 1] = px[1];
				part.Content[pos + 2] = px[2];
				if ( part.Channels == QoiChannels.Rgba )
					part.Content[pos + 3] = px[3];
			}

			reader.Skip( 8 );

			return part;
		}
	}

	public struct DelayPart : IPart
	{
		public PartType Type { get; }
		public float Delay;

		public DelayPart( float delay )
		{
			Type = PartType.SetDelayFloat;
			Delay = delay;
		}
	}

	public static AnimationPack Read( byte[] content )
	{
		var pack = new AnimationPack();

		var reader = new Reader( content );

		// Read Format Header
		{
			if ( content.Length < 4 )
				throw new AnimationPackException( "File too small - can't be AnimationPack" );
			if ( !(reader.Byte() == 'A' && reader.Byte() == 'N' && reader.Byte() == 'M') )
				throw new AnimationPackException( "Unknown magic - not AnimationPack!" );
			var version = reader.Byte();
			if ( version is < MinVersion or > MaxVersion )
				throw new AnimationPackException( $"Unknown AnimationPack version {version}" );
			pack.FileVersion = version;
		}

		// Read Animation Header
		{
			pack.FramesPerSecond = reader.Int32();
			pack.PartCount = reader.Int32();
		}

		Log.Info( pack.FramesPerSecond );
		Log.Info( pack.PartCount );

		for ( var i = 0; i < pack.PartCount; i++ )
		{
			var type = reader.Byte();
			switch ( (PartType)type )
			{
				case PartType.Unknown:
					throw new AnimationPackException( $"Uninitialized part type found while reading ({reader.Index})" );
				case PartType.Image:
					pack.Parts.Add( ImagePart.Read( ref reader, pack ) );
					break;
				case PartType.SetDelayFloat:
					reader.Skip( 4 ); // Skip part size
					pack.Parts.Add( new DelayPart( reader.Float() ) );
					break;
				default:
					throw new AnimationPackException(
						$"Unknown part type {type} found while reading ({reader.Index})" );
			}
		}

		return pack;
	}
}

public class AnimationPackException : Exception
{
	public AnimationPackException() { }

	public AnimationPackException( string message )
		: base( message )
	{
	}

	public AnimationPackException( string message, Exception inner )
		: base( message, inner )
	{
	}
}
