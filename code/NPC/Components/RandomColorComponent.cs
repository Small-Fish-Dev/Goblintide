using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameJam;

public enum ColorType
{
	Shirts,
	Trousers
}

[Prefab]
public class RandomColorComponent : EntityComponent<ModelEntity>
{
	public Dictionary<Color, float> Colors = new Dictionary<Color, float>()
	{
		{ new Color(0.21f, 0.08f, 0.08f), 0.1f },	// dull red
		{ new Color(0.27f, 0.20f, 0.12f), 5f },		// dull brown
		{ new Color(0.38f, 0.47f, 0.28f), 0.5f },	// dull green
		{ new Color(0.28f, 0.28f, 0.28f), 5f },		// dull grey
		{ new Color(0.60f, 0.48f, 0.29f), 1f },		// yellowish brown
		{ new Color(0.80f, 0.77f, 0.70f), 7f },		// off white
	};

	protected override void OnActivate()
	{
		Entity.RenderColor = WeightedList.RandomKey( Colors );
	}

}
