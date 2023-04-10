using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoblinGame;

[Prefab]
public partial class RandomMaterialGroupComponent : EntityComponent<ModelEntity>
{
	[Prefab]
	public virtual bool WeightedMaterialGroups { get; set; } = false;

	[Prefab]
	public virtual Dictionary<int, float> MaterialGroupsAndWeight { get; set; }
	protected override void OnActivate()
	{
		if ( !Game.IsServer ) return;

		int MaterialGroup = 0;

		if ( WeightedMaterialGroups && MaterialGroupsAndWeight.Count > 0 )
		{
			MaterialGroup = WeightedList.RandomKey( MaterialGroupsAndWeight );
			Log.Info( $"Material group {MaterialGroup} was chosen" );
		}
		else MaterialGroup = new Random().Int( 0, Entity.MaterialGroupCount );
		Entity.SetMaterialGroup( MaterialGroup );
	}
}
