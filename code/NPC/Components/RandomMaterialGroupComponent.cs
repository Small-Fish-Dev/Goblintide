using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameJam;

[Prefab]
public partial class RandomMaterialGroupComponent : EntityComponent<ModelEntity>
{
	protected override void OnActivate()
	{
		Entity.SetMaterialGroup( new Random().Int( 0, Entity.MaterialGroupCount ) );
	}
}
