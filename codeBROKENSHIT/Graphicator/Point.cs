using Vector2 = System.Numerics.Vector2;

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
	private System.Numerics.Vector2 Vector;

	public float X
	{
		readonly get => Vector.X;
		set => Vector.X = value;
	}

	public float Y
	{
		readonly get => Vector.Y;
		set => Vector.Y = value;
	}

	private PositionType _typeX;
	private PositionType _typeY;

	private Point( System.Numerics.Vector2 vector ) => Vector = vector;

	/// <summary> Create a new Point with coordinates based on 1920 x 1080 </summary>
	public Point( float x, float y ) : this( new System.Numerics.Vector2( x, y ) )
	{
	}

	/// <summary> Create a new Point with coordinates based on 1920 x 1080 </summary>
	public Point( float v ) : this( v, v )
	{
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
	private System.Numerics.Vector2 Calculate()
	{
		var vec2 = new System.Numerics.Vector2();
		switch ( _typeX )
		{
			case PositionType.AtPixelNative:
				vec2.X = X;
				break;
			case PositionType.Percentage:
				vec2.X = Screen.Width * X;
				break;
			case PositionType.AtPixel:
				{
					var v = X / 1920;
					vec2.X = v * Screen.Width;
					break;
				}
			default:
				throw new ArgumentOutOfRangeException();
		}

		switch ( _typeY )
		{
			case PositionType.AtPixelNative:
				vec2.Y = Y;
				break;
			case PositionType.Percentage:
				vec2.Y = Screen.Height * Y;
				break;
			case PositionType.AtPixel:
				{
					var v = Y / 1080;
					vec2.Y = v * Screen.Height;
					break;
				}
			default:
				throw new ArgumentOutOfRangeException();
		}

		return vec2;
	}

	public static implicit operator Point( System.Numerics.Vector2 value ) => new(value);
	public static implicit operator Point( global::Vector2 value ) => new(value);

	public static implicit operator System.Numerics.Vector2( Point v ) => v.Calculate();
	public static implicit operator global::Vector2( Point v ) => v.Calculate();

	public static implicit operator Point( double value ) => new((float)value, (float)value);
	public static implicit operator Point( float value ) => new(value, value);

	public static Point operator +( Point a, System.Numerics.Vector2 b ) => a.Vector + b;
	public static Point operator -( Point a, System.Numerics.Vector2 b ) => a.Vector - b;
	public static Point operator /( Point a, System.Numerics.Vector2 b ) => a.Vector / b;
	public static Point operator *( Point a, System.Numerics.Vector2 b ) => a.Vector * b;

	public static Point operator +( Point a, Point b ) => a.Vector + b.Vector;
	public static Point operator -( Point a, Point b ) => a.Vector - b.Vector;
	public static Point operator /( Point a, Point b ) => a.Vector / b.Vector;
	public static Point operator *( Point a, Point b ) => a.Vector * b.Vector;
}
