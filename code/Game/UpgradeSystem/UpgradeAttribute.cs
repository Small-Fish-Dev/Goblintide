namespace GameJam.UpgradeSystem;

[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public class UpgradeAttribute : Attribute
{
	public string Name;
	public string Dependency;
	public float Cost = 10;

	public UpgradeAttribute( string name ) => Name = name;
}
