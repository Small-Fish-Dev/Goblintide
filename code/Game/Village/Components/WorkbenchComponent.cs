namespace GameJam;

[Prefab]
public partial class WorkbenchComponent : StructureComponent
{
	public override void Initialize()
	{
		GameMgr.WeaponsPerSecond += 1d / 60d;
	}

	public override void OnDestroy()
	{
		GameMgr.WeaponsPerSecond -= 1d / 60d;
	}

	TimeUntil nextWeapon = 5f;

	[Event.Tick.Server]
	public void GenerateWeapons()
	{
		if ( nextWeapon )
		{
			nextWeapon = 60f;

			var doesntHaveWeapon = GameMgr.GoblinArmy
				.Where( x => !x.Weapon.IsValid() )
				.OrderBy( x => x.Position.DistanceSquared( Entity.Position ) );

			if ( doesntHaveWeapon.Count() > 0 )
			{
				var chosenKey = 0;
				if ( Lord.CombinedUpgrades != null )
					chosenKey = (int)Lord.CombinedUpgrades.Weapons;

				var chosenWeapon = BaseItem.FromPrefab( WeightedList.RandomKey( GameMgr.WeaponsList[chosenKey] ));

				if ( chosenWeapon.IsValid() )
					doesntHaveWeapon.First().Equip( chosenWeapon );
			}
		}
	}
}
