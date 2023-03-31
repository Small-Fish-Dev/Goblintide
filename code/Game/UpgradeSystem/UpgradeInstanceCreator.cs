namespace GameJam.UpgradeSystem;

public struct UpgradeInstanceCreator
{
	public TypeDescription Type;
	public UpgradeAttribute Attribute;

	private static readonly List<UpgradeInstanceCreator> Known = new();

	public static void RepopulateKnownUpgrades()
	{
		foreach ( var (type, attribute) in TypeLibrary.GetTypesWithAttribute<UpgradeAttribute>() )
		{
			var v = new UpgradeInstanceCreator { Type = type, Attribute = attribute };
			Known.Add( v );
		}
	}

	private static UpgradeInstanceCreator? Find( string name )
	{
		return Known.FirstOrDefault( v => v.Attribute.Name == name );
	}

	public Upgrade Create() => Type.Create<Upgrade>();
}
