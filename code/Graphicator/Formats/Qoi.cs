using System.Buffers.Binary;

namespace Graphicator.Formats;

public static class WhitelistedMemory
{
	public static uint ReadUInt32BigEndian( ReadOnlySpan<byte> a ) =>
		(uint)((a[0] << 24) | (a[1] << 16) | (a[2] << 8) | a[3]);
}

public static class Qoi
{
	#region Format

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

	#endregion

	#region Implementation

	public struct Image
	{
		public byte[] Data;
		public uint Width;
		public uint Height;
		public QoiChannels Channels;
		public QoiColorspace Colorspace;

		public Texture CreateGameTexture() =>
			Texture.Create( (int)Width, (int)Height )
				.WithData( Data )
				.WithFormat( ImageFormat.RGBA8888 )
				.Finish();

		public override string ToString() => $"{Width}x{Height} QOI image, {Channels}, {Colorspace}";
	}

	public static Image Read( ReadOnlySpan<byte> content )
	{
		var result = new Image();
		var size = content.Length;

		// Get some stuff ready
		var chunkContentLength = size - 8;

		// Read QOI magic
		if ( !(content[0] == 'q' && content[1] == 'o' && content[2] == 'i' && content[3] == 'f') )
			throw new ReadException( "QOI magic invalid - file isn't a QOI image or is corrupted" );

		// Read QOI header, initialize Image
		result.Width = WhitelistedMemory.ReadUInt32BigEndian( content[4..] );
		result.Height = WhitelistedMemory.ReadUInt32BigEndian( content[8..] );
		result.Channels = (QoiChannels)content[12];
		result.Colorspace = (QoiColorspace)content[13];

		// Initialize Image data
		result.Data = new byte[result.Width * result.Height * (byte)result.Channels];

		// Iteration variables
		var index = new byte[64 * 4]; // Previously seen pixel value array
		var px = new byte[] { 0, 0, 0, 255 };
		var run = 0;
		var p = 14;

		for ( var pos = 0; pos < result.Data.Length; pos += (byte)result.Channels )
		{
			if ( run > 0 )
				run--;
			else if ( p < chunkContentLength )
			{
				var b1 = content[p++];

				switch ( b1 )
				{
					case (byte)QoiOp.Rgb:
						px[0] = content[p++];
						px[1] = content[p++];
						px[2] = content[p++];
						break;
					case (byte)QoiOp.Rgba:
						px[0] = content[p++];
						px[1] = content[p++];
						px[2] = content[p++];
						px[3] = content[p++];
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
								var b2 = content[p++];
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

			result.Data[pos + 0] = px[0];
			result.Data[pos + 1] = px[1];
			result.Data[pos + 2] = px[2];
			if ( result.Channels == QoiChannels.Rgba )
				result.Data[pos + 3] = px[3];
		}

		return result;
	}

	#endregion

	public class ReadException : Exception
	{
		public ReadException() { }

		public ReadException( string message )
			: base( message )
		{
		}

		public ReadException( string message, Exception inner )
			: base( message, inner )
		{
		}
	}
}
