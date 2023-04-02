using GameJam.UpgradeSystem;

namespace GameJam;

public partial class Lord
{
	[Net, Change( "NetUpgradesEvent" )] private List<string> Upgrades { get; set; } = new();

	private Upgrade _combinedUpgrades;
	public Upgrade CombinedUpgrades => _combinedUpgrades;

	private void NetUpgradesEvent() => CombineUpgrades();

	public void CombineUpgrades()
	{
		_combinedUpgrades = Upgrade.CreateEmptyUpgrade();
		foreach ( var identifier in Upgrades )
		{
			var upgrade = Upgrade.Find( identifier );
			if ( upgrade == null ) throw new Exception( $"Unknown upgrade {identifier}" );
			upgrade.ForwardEffects( _combinedUpgrades );
		}
	}

	public void AddUpgrade( string identifier )
	{
		Game.AssertServer();
		if ( !Upgrade.Exists( identifier ) )
			throw new Exception( $"Unknown upgrade {identifier}" );
		Upgrades.Add( identifier );
		CombineUpgrades();
	}

	public void RemoveUpgrade( string identifier )
	{
		Game.AssertServer();
		if ( !Upgrade.Exists( identifier ) )
			throw new Exception( $"Unknown upgrade {identifier}" );
		Upgrades.Remove( identifier );
		CombineUpgrades();
	}

	public bool HasUpgrade( string identifier ) => Upgrades.Contains( identifier );
}
