global using System.IO;

namespace GoblinGame.Persistence;

public static class PersistenceManager
{
	/// <summary>
	/// Path to current save.
	/// </summary>
	public const string SAVE_PATH = "./save_rework.dat";

	/// <summary>
	/// Instances of all the converters.
	/// </summary>
	public static Dictionary<Type, object> Converters =>
		TypeLibrary?.GetTypesWithAttribute<ConverterAttribute>()
			.Where( tuple => !tuple.Type.IsAbstract )
			.Select( tuple => TypeLibrary.Create( tuple.Type.FullName, tuple.Type.TargetType ) )
			.ToDictionary( obj => (Type)TypeLibrary.GetType( obj.GetType() ).GetProperty( "GenericType" ).GetValue( obj ) );

	private static List<object> persistTargets = new();

	/// <summary>
	/// Registers a non-entity class for persistance.
	/// </summary>
	/// <param name="obj"></param>
	public static void Register( object? obj )
	{
		if ( persistTargets.Contains( obj ) && TypeLibrary.HasAttribute<PersistType>( obj.GetType() ) )
			return;

		persistTargets.Add( obj );
	}

	/// <summary>
	/// Unegisters a non-entity class for persistance.
	/// </summary>
	/// <param name="obj"></param>
	public static void Unregister( object? obj )
	{
		if ( !persistTargets.Contains( obj ) )
			return;

		persistTargets.Remove( obj );
	}

	public static void Load()
	{
		// Don't load if there's nothing to load.
		if ( !FileSystem.Data.FileExists( SAVE_PATH ) )
		{
			// Do we need to do initial stuff if the save doesn't exist?
			return;
		}

		// The tools we're going to need to read our values from binary.
		using var stream = FileSystem.Data.OpenRead( SAVE_PATH );
		using var reader = new BinaryReader( stream );

		// Cache converters locally.
		var converters = Converters;

		// Go through all instances our stream has.
		var instances = reader.ReadInt32();

		for ( int i = 0; i < instances; i++ )
		{
			// Read initial information of an object.
			var objName = reader.ReadString();
			var propertyCount = reader.ReadInt32();

			var type = TypeLibrary.GetType( objName );
			if ( type == null || !type.HasAttribute<PersistType>() )
				continue;

			// Create a new instance and go through all the persisted properties.
			try
			{
				// We don't want to create an instance if the class is only static.
				var instance = type.IsStatic 
					? null
					: TypeLibrary.Create( type.FullName, type.TargetType );

				for ( int j = 0; j < propertyCount; j++ )
				{
					// Read property information.
					var name = reader.ReadString();
					var property = type.GetProperty( name );
					if ( property == null || !property.CanRead || !property.CanWrite )
						continue;

					// Get the type converter.
					var valueType = property.PropertyType;
					if ( !converters.TryGetValue( valueType, out var converter ) )
					{
						Log.Error( $"[PERSISTENCE] Converter for type \"{type}\" not found." );
						continue;
					}

					// Get the converter's method for reading the current property type.
					var typeDescription = TypeLibrary.GetType( converter.GetType() );
					var method = typeDescription.GetMethod( "Read" );
					if ( method == null )
						continue;

					// Read value and assign it to the property.
					var value = method?.InvokeWithReturn<object>( converter, new[] { reader } );
					property.SetValue( instance, value );
				}
			}
			catch ( Exception ex )
			{
				Log.Error(
					$"[PERSISTENCE] Please send the following error to @ceitine#2355.\n"
					+ $"{ex.Message} {ex.StackTrace}" );
			}
		}
	}

	public static void Save()
	{
		// The tools we're going to need to write our values in binary.
		using var stream = FileSystem.Data.OpenWrite( SAVE_PATH, FileMode.OpenOrCreate );
		using var writer = new BinaryWriter( stream );

		// Get all instances that should persist.
		var instances = Entity.All
			.Where( e => TypeLibrary.HasAttribute<PersistType>( e.GetType() ) )
			.Concat( persistTargets );

		// Cache converters locally.
		var converters = Converters;

		// Loop through all those instances and write them to the memory stream.
		writer.Write( instances.Count() );

		foreach ( var obj in instances )
		{
			if ( obj == null )
				continue;

			// Make sure the object has properties to be persisted.
			var properties = TypeLibrary.GetPropertyDescriptions( obj )
				.Where( p => p.HasAttribute<PersistProperty>() );
			var count = properties.Count();
			if ( count < 0 ) 
				continue;

			// Write initial information of a object.
			writer.Write( obj.GetType().FullName );
			writer.Write( count );

			// Go through all persisted properties.
			foreach ( var property in properties )
			{
				if ( !property.CanRead || !property.CanWrite )
					continue;

				// Get the property values if possible, convert them to binary and write the name and value.
				try
				{
					// Try and get basic information of the property.
					var value = property?.GetValue( obj );
					var type = value?.GetType();
					if ( type == null )
						continue;

					// Get the type converter.
					if ( !converters.TryGetValue( type, out var converter ) )
					{
						Log.Error( $"[PERSISTENCE] Converter for type \"{type}\" not found." );
						continue;
					}

					// Write property information and use the converter's method to write the value.
					var typeDescription = TypeLibrary.GetType( converter.GetType() );
					var method = typeDescription.GetMethod( "Write" );
					if ( method == null )
						continue;

					// Write the name and value of the property.
					writer.Write( property.Name );
					method?.Invoke( converter, new[] { writer, value } );
				}
				catch ( Exception ex )
				{
					Log.Error( 
						$"[PERSISTENCE] Please send the following error to @ceitine#2355.\n"
						+ $"{ex.Message} {ex.StackTrace}" );
				}
			}
		}
	}
}
