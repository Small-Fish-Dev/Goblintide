namespace GoblinGame.Persistence;

// Hack for getting types that inherit a generic type.
[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = true )]
public class ConverterAttribute : Attribute
{

}

[Converter]
public abstract class BaseConverter<T>
{
	/// <summary>
	/// The type of generic this converter is.
	/// </summary>
	public Type GenericType => typeof( T );

	/// <summary>
	/// Turn binary into an object.
	/// </summary>
	/// <param name="reader"></param>
	/// <returns></returns>
	public abstract T Read( BinaryReader reader );

	/// <summary>
	/// Turn the object into binary.
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="value"></param>
	public abstract void Write( BinaryWriter writer, T value );
}

// Some default converters.
public class Int32Converter : BaseConverter<int>
{
	public override int Read( BinaryReader reader ) 
		=> reader.ReadInt32();

	public override void Write( BinaryWriter writer, int value )
		=> writer.Write( value );
}

public class Int64Converter : BaseConverter<long>
{
	public override long Read( BinaryReader reader )
		=> reader.ReadInt64();

	public override void Write( BinaryWriter writer, long value )
		=> writer.Write( value );
}

public class StringConverter : BaseConverter<string>
{
	public override string Read( BinaryReader reader )
		=> reader.ReadString();

	public override void Write( BinaryWriter writer, string value )
		=> writer.Write( value );
}

public class FloatConverter : BaseConverter<float>
{
	public override float Read( BinaryReader reader )
		=> reader.ReadSingle();

	public override void Write( BinaryWriter writer, float value )
		=> writer.Write( value );
}
