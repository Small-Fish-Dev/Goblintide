namespace GameJam.UpgradeSystem;

public partial class UpgradableComponent : EntityComponent
{
	private readonly List<(Upgrade, string)> _upgrades = new();
	public IEnumerable<(Upgrade, string)> Upgrades => _upgrades.AsReadOnly();

	[ClientRpc]
	private void AddClient( string name )
	{
		AddByName( name );
	}

	private void AddByName( string name )
	{
		var creator = UpgradeInstanceCreator.Find( name );
		if ( creator == null ) throw new Exception( $"Creator for {name} not found" );
		AddToInstance( creator.Value );
	}

	private void AddToInstance( UpgradeInstanceCreator creator ) =>
		_upgrades.Add( (creator.Create(), creator.Attribute.Name) );

	public void Add( UpgradeInstanceCreator creator )
	{
		AddToInstance( creator );
		AddClient( creator.Attribute.Name );
	}
}
