using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameGame;

[Prefab, Category("items")]

public partial class Seat : ModelEntity
{
	public override void OnActive()
	{
		SetModel( "models/editor/axis_helper.vmdl_c" );
	}
}
