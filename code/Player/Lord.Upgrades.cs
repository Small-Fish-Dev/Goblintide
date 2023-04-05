using GameJam.UpgradeSystem;

namespace GameJam;

public partial class Lord
{
	[Net, Change( "NetUpgradesEvent" )] public List<string> Upgrades { get; set; } = new();

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

	public void SimulateUpgrades()
	{
		if ( !Game.IsServer ) return;
		if ( CombinedUpgrades == null ) return;

		var allNPCs = Entity.All
			.OfType<BaseNPC>();

		var allAllies = allNPCs
				.Where( x => x.Faction == Faction );

		var allEnemies = allNPCs
				.Where( x => x.Faction != Faction );

		var closeAllies = allAllies
			.Where( x => x.FastRelativeInRangeCheck( this, 400f ) );

		var closeEnemies = allEnemies
			.Where( x => x.FastRelativeInRangeCheck( this, 400f ) );

		if ( CombinedUpgrades.AuraOfFear > 0f )
		{
			foreach( var enemy in closeEnemies )
			{
				enemy.BaseDiligency = enemy.RootPrefab.GetValue<float>( "BaseDiligency" ) - CombinedUpgrades.AuraOfFear;
			}
		}

		if ( CombinedUpgrades.AuraOfRespect > 0f )
		{
			foreach ( var ally in closeAllies )
			{
				ally.BaseDiligency = ally.RootPrefab.GetValue<float>( "BaseDiligency" ) + CombinedUpgrades.AuraOfRespect;
			}
		}
	}
}
