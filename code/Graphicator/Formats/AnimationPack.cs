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
