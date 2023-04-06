using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameJam;

public enum AccessoryType
{
	MaleHair,
	FemaleHair,
	Eyebrows,
	FacialHair,
	ClothingExtras,
	GoblinExtras
}

[Prefab]
public partial class RandomAccessoryComponent : CharacterComponent
{

	public static List<string> MaleHairs = new()
	{
		"models/citizen_clothes/hair/hair_thinning_short/models/thinning_short_red.vmdl_c",
		"models/citizen_clothes/hair/hair_thinning_short/models/thinning_short_grey.vmdl_c",
		"models/citizen_clothes/hair/hair_thinning_short/models/thinning_short_blonde.vmdl_c",
		"models/citizen_clothes/hair/hair_thinning_short/models/thinning_short.vmdl_c",
		"models/citizen_clothes/hair/hair_shortscruffy/models/hair_shortscruffy_red.vmdl_c",
		"models/citizen_clothes/hair/hair_shortscruffy/models/hair_shortscruffy_grey.vmdl_c",
		"models/citizen_clothes/hair/hair_shortscruffy/models/hair_shortscruffy_blonde.vmdl_c",
		"models/citizen_clothes/hair/hair_shortscruffy/models/hair_shortscruffy_black.vmdl_c",
		"models/citizen_clothes/hair/hair_shortscruffy/models/hair_shortscruffy.vmdl_c",
		"models/citizen_clothes/hair/hair_longbrown/models/hair_longred.vmdl_c",
		"models/citizen_clothes/hair/hair_longbrown/models/hair_longgrey.vmdl_c",
		"models/citizen_clothes/hair/hair_longbrown/models/hair_longblonde.vmdl_c",
		"models/citizen_clothes/hair/hair_longbrown/models/hair_longblack.vmdl_c",
		"models/citizen_clothes/hair/hair_longbrown/models/hair_longbrown.vmdl_c",
		"models/citizen_clothes/hair/hair_balding/models/hair_baldingbrown.vmdl_c",
		"models/citizen_clothes/hair/hair_balding/models/hair_baldinggrey.vmdl_c",
		"models/citizen_clothes/hair/hair_wavyblack/model/hair_wavyblack.vmdl_c",
		"",
		"",
		"",
		"",
	};

	public static List<string> Beards = new()
	{
		"models/citizen_clothes/hair/stubble/model/stubble.vmdl_c",
		"models/citizen_clothes/hair/scruffy_beard/models/scruffy_beard_grey.vmdl_c",
		"models/citizen_clothes/hair/scruffy_beard/models/scruffy_beard_brown.vmdl_c",
		"models/citizen_clothes/hair/scruffy_beard/models/scruffy_beard_black.vmdl_c",
		"models/citizen_clothes/hair/mutton_chops/models/mutton_chops.vmdl_c",
		"models/citizen_clothes/hair/moustache/models/moustache_grey.vmdl_c",
		"models/citizen_clothes/hair/moustache/models/moustache_brown.vmdl_c",
		"models/citizen_clothes/hair/goatee/models/goatee_grey.vmdl_c",
		"models/citizen_clothes/hair/goatee/models/goatee_black.vmdl_c",
		"models/citizen_clothes/hair/goatee/models/goatee.vmdl_c",
		"",
		"",
		"",
	};

	public static List<string> Eyebrows = new()
	{
		"models/citizen_clothes/hair/eyebrows/models/eyebrows.vmdl_c",
		"models/citizen_clothes/hair/eyebrows/models/eyebrows_black.vmdl_c",
		"models/citizen_clothes/hair/eyebrows_bushy/models/eyebrows_bushy.vmdl_c",
		"models/citizen_clothes/hair/eyebrows_bushy/models/eyebrows_bushy.vmdl_c",
		"models/citizen_clothes/hair/eyebrows_monobrow/models/monobrow.vmdl_c",
		"models/citizen_clothes/hair/eyebrows_monobrow/models/monobrow/monobrow_grey.vmdl_c",
		"",
	};

	public static List<string> FemaleHair = new()
	{
		"models/citizen_clothes/hair/hair_bobcut/models/hair_bobcut.vmdl_c",
		"models/citizen_clothes/hair/hair_bobcut/models/hair_bobcut_black.vmdl_c",
		"models/citizen_clothes/hair/hair_bobcut/models/hair_bobcut_brown.vmdl_c",
		"models/citizen_clothes/hair/hair_bobcut/models/hair_bobcut_red.vmdl_c",
		"models/citizen_clothes/hair/hair_bun/models/hair_bun.vmdl_c",
		"models/citizen_clothes/hair/hair_bun/models/hair_bun_black.vmdl_c",
		"models/citizen_clothes/hair/hair_bun/models/hair_bun_blonde.vmdl_c",
		"models/citizen_clothes/hair/hair_bun/models/hair_bun_brown.vmdl_c",
		"models/citizen_clothes/hair/hair_flower/models/hair_flower.vmdl_c",
		"models/citizen_clothes/hair/hair_flower/models/hair_flower_black.vmdl_c",
		"models/citizen_clothes/hair/hair_flower/models/hair_flower_blonde.vmdl_c",
		"models/citizen_clothes/hair/hair_flower/models/hair_flower_red.vmdl_c",
		"models/citizen_clothes/hair/hair_longcurly/models/hair_longcurly.vmdl_c",
		"models/citizen_clothes/hair/hair_longcurly/models/hair_longcurly_black.vmdl_c",
		"models/citizen_clothes/hair/hair_longcurly/models/hair_longcurly_blonde.vmdl_c",
		"models/citizen_clothes/hair/hair_longcurly/models/hair_longcurly_brown.vmdl_c",
		"models/citizen_clothes/hair/hair_ponytail/models/hair_ponytail.vmdl_c",
		"models/citizen_clothes/hair/hair_ponytail/models/hair_ponytail_black.vmdl_c",
		"models/citizen_clothes/hair/hair_ponytail/models/hair_ponytail_blonde.vmdl_c",
		"models/citizen_clothes/hair/hair_ponytail/models/hair_ponytail_grey.vmdl_c",
		"models/citizen_clothes/hair/hair_ponytail/models/hair_ponytail_red.vmdl_c",
		"models/citizen_clothes/hair/hair_tight_ponytail/models/hair_tight_ponytail_black.vmdl_c",
		"models/citizen_clothes/hair/hair_tight_ponytail/models/hair_tight_ponytail_blonde.vmdl_c",
		"models/citizen_clothes/hair/hair_tight_ponytail/models/hair_tight_ponytail_brown.vmdl_c",
		"models/citizen_clothes/hair/hair_tight_ponytail/models/hair_tight_ponytail_grey.vmdl_c",
		"models/citizen_clothes/hair/hair_tight_ponytail/models/hair_tight_ponytail_red.vmdl_c",
	};

	public static List<string> ClothingExtras = new()
	{
		"",
		"models/clothes/inner_shirt/inner_shirt.vmdl"
	};

	public static List<string> GoblinExtras = new()
	{
		"",
		"",
		"",
		"models/goblin/accessories/gobo_necklace.vmdl",
	};

	[Prefab]
	public AccessoryType Type { get; set; }

	private ModelEntity Accessory { get; set; }
	protected override void OnActivate()
	{
		if ( !Game.IsServer ) return;

		Accessory = new ModelEntity();
		var model = GetAccessoryFromType( Type );
		if ( model == "" ) Accessory.Delete();
		else
		{
			Accessory.SetModel( GetAccessoryFromType( Type ) );
			Accessory.SetParent( Entity, true );
		}
		
	}

	private string GetAccessoryFromType( AccessoryType accessoryType )
	{
		switch( accessoryType )
		{
			case AccessoryType.MaleHair:
				return new Random().FromList( MaleHairs );
			case AccessoryType.FemaleHair:
				return new Random().FromList( FemaleHair );
			case AccessoryType.FacialHair:
				return new Random().FromList( Beards );
			case AccessoryType.Eyebrows:
				return new Random().FromList( Eyebrows );
			case AccessoryType.ClothingExtras:
				return new Random().FromList( ClothingExtras );
			case AccessoryType.GoblinExtras:
				return new Random().FromList( GoblinExtras );
			default:
				return "";

		}
	}
}
