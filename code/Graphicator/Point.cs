namespace Graphicator;

public enum PositionType
{
	/// <summary> Positioned at screen coordinates based on 16:9 1920 x 1080 </summary>
	AtPixel,

	/// <summary> Positioned at exact screen coordinates </summary>
	AtPixelNative,

	/// <summary> Percentage value 0 - 1 </summary>
	Percentage
}

public struct Point
{
	public float X;
	public float Y;

	private PositionType _typeX;
	private PositionType _typeY;

	/// <summary> Create a new Point with coordinates based on 1920 x 1080 </summary>
	public Point( float x, float y )
	{
		X = x;
		Y = y;
		_typeX = PositionType.AtPixel;
		_typeY = PositionType.AtPixel;
	}

	public Point WithHorizontalType( PositionType type )
	{
		_typeX = type;
		return this;
	}

	public Point WithVerticalType( PositionType type )
	{
		_typeY = type;
		return this;
	}

	public Point WithType( PositionType type )
	{
		_typeX = type;
		_typeY = type;
		return this;
	}

	/// <summary>
	/// Get screen coordinates from the Point
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">Invalid position type</exception>
	private Vector2 Calculate()
	{
		var vec2 = new Vector2();
		switch ( _typeX )
		{
			case PositionType.AtPixelNative:
				vec2.x = X;
				break;
			case PositionType.Percentage:
				vec2.x = Screen.Width * X;
				break;
			case PositionType.AtPixel:
				{
					var v = X / 1920;
					vec2.x = v * Screen.Width;
					break;
				}
			default:
				throw new ArgumentOutOfRangeException();
		}

		switch ( _typeY )
		{
			case PositionType.AtPixelNative:
				vec2.y = Y;
				break;
			case PositionType.Percentage:
				vec2.y = Screen.Height * Y;
				break;
			case PositionType.AtPixel:
				{
					var v = Y / 1080;
					vec2.y = v * Screen.Height;
					break;
				}
			default:
				throw new ArgumentOutOfRangeException();
		}

		return vec2;
	}

	public static implicit operator Vector2( Point v ) => v.Calculate();
}
