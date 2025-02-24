using System.Runtime.CompilerServices;

namespace GoblinGame.Util;

/// <summary>
/// An integer between two values, which can be randomized or fixed.
/// </summary>
public class RangedInt
{
	/// <summary>
	/// Range type of RangedInt.
	/// </summary>
	public enum RangeType
	{
		/// <summary>
		/// Single value, both minimum and maximum value.
		/// </summary>
		[Icon( "fiber_manual_record" )]
		Fixed,
		/// <summary>
		/// Random value between given minimum and maximum.
		/// </summary>
		[Icon( "join_full" )]
		Between
	}

	/// <summary>
	/// The minimum value of the int range.
	/// </summary>
	public int x;

	/// <summary>
	/// The maximum value of the int range.
	/// </summary>
	public int y;

	/// <summary>
	/// Range type of this int.
	/// </summary>
	public RangeType Range { get; set; }

	/// <summary>
	/// Initialize the int as a fixed value.
	/// </summary>
	/// <param name="fixedValue">The fixed value for this int</param>
	public RangedInt( int fixedValue )
	{
		x = fixedValue;
		y = fixedValue;
		Range = RangeType.Fixed;
	}

	/// <summary>
	/// Initialize the int as a random value between given min and max.
	/// </summary>
	/// <param name="min">The minimum possible value for this int.</param>
	/// <param name="max">The maximum possible value for this int.</param>
	public RangedInt( int min, int max )
	{
		x = min;
		y = max;
		Range = RangeType.Between;
	}

	/// <summary>
	/// </summary>
	/// <returns>The final value of this ranged int, randomizing between min and max values.</returns>
	public int GetValue()
	{
		if ( Range == RangeType.Between )
		{
			return Game.Random.Int( x, y );
		}

		return x;
	}

	/// <summary>
	/// Parse a ranged int from a string. Format is "min max rangetype".
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static RangedInt Parse( string str )
	{
		str = str.Trim( '[', ']', ' ', '\n', '\r', '\t', '"' );
		string[] array = str.Split( new char[5] { ' ', ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries );
		RangedInt result;
		if ( array.Length != 3 )
		{
			result = default;
			return result;
		}

		result = default;
		result.x = array[0].ToInt();
		result.y = array[1].ToInt();
		result.Range = (RangeType)array[2].ToInt();
		return result;
	}

	public override string ToString()
	{
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new( 2, 3 );
		defaultInterpolatedStringHandler.AppendFormatted( x );
		defaultInterpolatedStringHandler.AppendLiteral( "," );
		defaultInterpolatedStringHandler.AppendFormatted( y );
		defaultInterpolatedStringHandler.AppendLiteral( "," );
		defaultInterpolatedStringHandler.AppendFormatted( (int)Range );
		return defaultInterpolatedStringHandler.ToStringAndClear();
	}

	public static implicit operator RangedInt( int input )
	{
		return new RangedInt( input );
	}
}
