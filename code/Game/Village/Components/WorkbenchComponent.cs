namespace GoblinGame;

[Prefab]
public partial class WorkbenchComponent : StructureComponent
{
	public override void Initialize()
	{
		Goblintide.WeaponsPerSecond += 1d / 60d;
	}

	public override void OnDestroy()
	{
		Goblintide.WeaponsPerSecond -= 1d / 60d;
	}

	TimeUntil nextWeapon = 5f;

	[Event.Tick.Server]
	public void GenerateWeapons()
	{
		if ( nextWeapon )
		{
			nextWeapon = 60f;

			var doesntHaveWeapon = Goblintide.GoblinArmy
				.Where( x => !x.Weapon.IsValid() )
				.OrderBy( x => x.Position.DistanceSquared( Entity.Position ) );

			if ( doesntHaveWeapon.Count() > 0 )
			{
				var chosenKey = 0;
				if ( Lord.CombinedUpgrades != null )
					chosenKey = (int)Lord.CombinedUpgrades.Weapons;

				var chosenWeapon = BaseItem.FromPrefab( WeightedList.RandomKey( Goblintide.WeaponsList[chosenKey] ));

				if ( chosenWeapon.IsValid() )
					doesntHaveWeapon.First().Equip( chosenWeapon );
			}
		}
	}
}
