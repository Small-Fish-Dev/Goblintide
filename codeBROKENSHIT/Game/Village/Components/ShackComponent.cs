namespace GoblinGame;

[Prefab]
public partial class ShackComponent : StructureComponent
{
	public override void Initialize() 
	{
		Goblintide.GoblinPerSecond += 1d / 1d;
	}

	public override void OnDestroy()
	{
		Goblintide.GoblinPerSecond -= 1d / 1d;
	}

	TimeUntil nextGoblin = 5f;

	[Event.Tick.Server]
	public void GenerateGoblin()
	{
		if ( nextGoblin )
		{
			nextGoblin = 60f;

			if ( Goblintide.GoblinArmy.Count() < Goblintide.MaxArmySize )
			{
				var gob = BaseNPC.FromPrefab( "prefabs/npcs/goblin.prefab" );
				if ( gob.IsValid() )
				{
					gob.Position = Entity.Position + Entity.Rotation.Backward * 100f;
					Goblintide.GoblinArmy.Add( gob );

					if ( Lord.CombinedUpgrades != null )
						if ( Lord.CombinedUpgrades.Milk > 0 )
							gob.SetLevel( 1 + (int)(Lord.CombinedUpgrades.Milk * 1.7f ) );
				}
			}
		}
	}
}
