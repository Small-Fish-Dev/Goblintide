namespace GoblinGame.Persistence;

/// <summary>
/// Use this attribute to mark a property of a persited object to be persisted.
/// </summary>
[AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
public class PersistProperty : Attribute
{

}
