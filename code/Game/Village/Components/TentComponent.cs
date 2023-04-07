namespace GameJam;

[Prefab]
public partial class TentComponent : StructureComponent
{
	public override void Initialize()
	{
		var extraSpace = 0;

		if ( Lord.CombinedUpgrades != null )
			if ( Lord.CombinedUpgrades.MacMansion > 0 )
				extraSpace =  (int)Lord.CombinedUpgrades.MacMansion;

		GameMgr.MaxArmySize += ( 10 + extraSpace );
	}

	public override void OnDestroy()
	{
		var extraSpace = 0;

		if ( Lord.CombinedUpgrades != null )
			if ( Lord.CombinedUpgrades.MacMansion > 0 )
				extraSpace = (int)Lord.CombinedUpgrades.MacMansion;

		GameMgr.MaxArmySize -= ( 10 + extraSpace );
	}
}
