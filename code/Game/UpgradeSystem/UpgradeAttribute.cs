namespace GameJam.UpgradeSystem;

[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public class UpgradeAttribute : Attribute
{
	public string Name;
	public string Dependency;

	public UpgradeAttribute( string name ) => Name = name;
}
