namespace GoblinGame;

[Prefab]
public partial class TentComponent : StructureComponent
{
	public override void Initialize()
	{
		var extraSpace = 0;

		if ( Lord.CombinedUpgrades != null )
			if ( Lord.CombinedUpgrades.MacMansion > 0 )
				extraSpace =  (int)Lord.CombinedUpgrades.MacMansion;

		Goblintide.MaxArmySize += ( 10 + extraSpace );
	}

	public override void OnDestroy()
	{
		var extraSpace = 0;

		if ( Lord.CombinedUpgrades != null )
			if ( Lord.CombinedUpgrades.MacMansion > 0 )
				extraSpace = (int)Lord.CombinedUpgrades.MacMansion;

		Goblintide.MaxArmySize -= ( 10 + extraSpace );
	}

	[Event( "UpgradeBought" )]
	public static void CheckNewMaxArmy( string identifier )
	{
		if ( identifier.StartsWith( "Mac Mansion" ) )
		{
			if ( Lord.CombinedUpgrades != null )
				if ( Lord.CombinedUpgrades.MacMansion > 0 )
					Goblintide.MaxArmySize++;
		}
	}
}
