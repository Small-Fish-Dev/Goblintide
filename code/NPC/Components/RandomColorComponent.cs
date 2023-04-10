using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoblinGame;

public enum ColorType
{
	Shirts,
	Trousers
}

[Prefab]
public class RandomColorComponent : EntityComponent<ModelEntity>
{
	public static Dictionary<Color, float> ShirtsColors = new Dictionary<Color, float>()
	{
		{ new Color(0.21f, 0.08f, 0.08f), 1f },	// dull red
		{ new Color(0.27f, 0.20f, 0.12f), 4f },		// dull brown
		{ new Color(0.38f, 0.47f, 0.28f), 4f },	// dull green
		{ new Color(0.28f, 0.28f, 0.28f), 5f },		// dull grey
		{ new Color(0.60f, 0.48f, 0.29f), 2f },		// yellowish brown
		{ new Color(0.80f, 0.77f, 0.70f), 7f },		// off white
		{ new Color(0.90f, 0.76f, 0.49f), 2f },		// slightly orange
		{ new Color(0.20f, 0.27f, 0.12f), 3f },		// dark green
	};

	public static Dictionary<Color, float> TrousersColors = new Dictionary<Color, float>()
	{
		{ new Color(0.32f, 0.27f, 0.176f), 3f },	// dull brown
		{ new Color(0.14f, 0.14f, 0.14f), 5f },		// dark grey
		{ new Color(0.94f, 0.89f, 0.85f), 1f }		// off white
	};

	[Prefab]
	public virtual ColorType colorType { get; set; }

	protected override void OnActivate()
	{
		if ( !Game.IsServer ) return;
		Entity.RenderColor = GetColorFromType( colorType );
	}

	public Color GetColorFromType( ColorType type)
	{
		switch(type)
		{
			case ColorType.Shirts:
				return WeightedList.RandomKey( ShirtsColors );
			case ColorType.Trousers:
				return WeightedList.RandomKey( TrousersColors );
			default:
				return Color.White;
		}
	}

}
