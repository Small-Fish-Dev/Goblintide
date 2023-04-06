using GameJam.UpgradeSystem;

namespace GameJam;

public partial class Lord
{
	[Net, Change( "NetUpgradesEvent" )] public List<string> Upgrades { get; set; } = new();

	private static Upgrade _combinedUpgrades;
	public static Upgrade CombinedUpgrades => _combinedUpgrades;

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
		Event.Run( "UpgradeBought", identifier );
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

	[ConCmd.Server( "gb_buyupgrade" )]
	public static void BuyUpgrade( string identifier )
	{
		Game.AssertServer();

		if ( ConsoleSystem.Caller.Pawn is not Lord caller )
			return;

		var upgrade = Upgrade.Find( identifier );
		if ( upgrade == null )
		{
			Log.Warning( $"{ConsoleSystem.Caller.Name} tried to buy unknown upgrade {identifier}" );
			return;
		}

		if ( GameMgr.TotalIQ < 1 )
		{
			Log.Warning( $"{ConsoleSystem.Caller.Name} doesn't have the funds for upgrade {identifier}" );
			return;
		}

		if ( caller.HasUpgrade( identifier ) )
		{
			Log.Warning( $"{ConsoleSystem.Caller.Name} already has upgrade {identifier}" );
			return;
		}

		GameMgr.TotalIQ--;
		caller.AddUpgrade( identifier );
	}

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
			foreach ( var enemy in allEnemies )
			{
				if ( enemy.RootPrefab != null )
					enemy.BaseDiligency = enemy.RootPrefab.GetValue<float>( "BaseDiligency" );
			}

			foreach ( var enemy in closeEnemies )
			{
				enemy.BaseDiligency -= CombinedUpgrades.AuraOfFear;
			}
		}

		if ( CombinedUpgrades.AuraOfRespect > 0f || CombinedUpgrades.GoblinSchool > 0f )
		{
			foreach ( var ally in allAllies )
			{
				if ( ally.RootPrefab != null )
					ally.BaseDiligency = ally.RootPrefab.GetValue<float>( "BaseDiligency" ) + CombinedUpgrades.GoblinSchool;
			}

			foreach ( var ally in closeAllies )
			{
				ally.BaseDiligency += CombinedUpgrades.AuraOfRespect;
			}
		}

		if ( CombinedUpgrades.BackseatGaming > 0f )
		{
			foreach ( var ally in allAllies )
			{
				if ( ally.RootPrefab != null )
					ally.AttackSpeed = ally.RootPrefab.GetValue<float>( "AttackSpeed" );
			}

			foreach ( var ally in closeAllies )
			{
				ally.AttackSpeed *= (1f + CombinedUpgrades.BackseatGaming);
			}
		}

		if ( CombinedUpgrades.Swiftness > 0f )
		{
			WalkSpeed = BaseWalkSpeed * (1f + CombinedUpgrades.Swiftness);
		}
	}
}
