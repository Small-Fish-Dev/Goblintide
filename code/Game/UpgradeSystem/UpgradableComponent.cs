namespace GameJam.UpgradeSystem;

public class UpgradableComponent : EntityComponent
{
	public List<(Upgrade, string)> Upgrades = new();
}
