namespace GoblinGame.Persistence;

/// <summary>
/// Use this attribute to mark a type to be persisted.
/// Note that the class must have a parameterless constructor.
/// </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public class PersistType : Attribute
{

}
