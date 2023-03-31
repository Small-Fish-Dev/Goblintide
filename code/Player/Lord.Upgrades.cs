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
		
	}

	public void AddUpgrade( string name )
	{
		
	}

	public void RemoveUpgrade( string name )
	{
		
	}
}
