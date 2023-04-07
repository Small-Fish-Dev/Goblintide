namespace GameJam.UpgradeSystem;

public static class Creator
{
	static Creator() => Build();

	[Event.Hotload]
	private static void Build()
	{
		Upgrade.ClearAll();
		new Upgrade.Builder( "Aura of Fear I", "Enemies around you have lower diligence." )
			.ConfigureWith( v =>
			{
				v.AuraOfFear = 0.05f; // -5%
			} )
			.WithTexture( "fear/fear1.png" )
			.Next( "Aura of Fear II",
				v =>
					v.WithTexture( "fear/fear2.png" )
					.PlaceAt( Vector2.Left * 150 ) )
			.Next( "Aura of Fear III",
				v =>
					v.WithTexture( "fear/fear3.png" )
						.PlaceAt( Vector2.Left * 300 ) )
			.Next( "Aura of Fear IV",
				v =>
					v.WithTexture( "fear/fear4.png" )
						.PlaceAt( Vector2.Left * 450 ) )
			.Next( "Aura of Fear V",
				v =>
					v.WithTexture( "fear/fear5.png" )
						.PlaceAt( Vector2.Left * 600 ) )
			.Build();

		new Upgrade.Builder( "Aura of Respect I", "Allies around you have higher diligence." )
			.ConfigureWith( v =>
			{
				v.AuraOfRespect = 0.05f; // +5%
			} )
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 150 )
			.Next( "Aura of Respect II",
				v =>
					v.WithTexture( "aura/aura2.png" )
					.PlaceAt( Vector2.Down * 150 + Vector2.Left * 150 ) )
			.Next( "Aura of Respect III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 150 + Vector2.Left * 300 ) )
			.Next( "Aura of Respect IV",
				v =>
					v.WithTexture( "aura/aura4.png" )
						.PlaceAt( Vector2.Down * 150 + Vector2.Left * 450 ) )
			.Next( "Aura of Respect V",
				v =>
					v.WithTexture( "aura/aura5.png" )
						.PlaceAt( Vector2.Down * 150 + Vector2.Left * 600 ) )
			.Build();

		new Upgrade.Builder( "Goblin School I", "Increase Base Goblin diligence." )
			.ConfigureWith( v =>
			{
				v.GoblinSchool = 0.05f; // +5%
			} )
			.WithTexture( "school/school1.png" )
			.PlaceAt( Vector2.Down * 300 )
			.Next( "Goblin School II",
				v =>
					v.WithTexture( "school/school2.png" )
						.PlaceAt( Vector2.Down * 300 + Vector2.Left * 150 ) )
			.Next( "Goblin School III",
				v =>
					v.WithTexture( "school/school3.png" )
						.PlaceAt( Vector2.Down * 300 + Vector2.Left * 300 ) )
			.Next( "Goblin School IV",
				v =>
					v.WithTexture( "school/school4.png" )
						.PlaceAt( Vector2.Down * 300 + Vector2.Left * 450 ) )
			.Next( "Goblin School V",
				v =>
					v.WithTexture( "school/school5.png" )
						.PlaceAt( Vector2.Down * 300 + Vector2.Left * 600 ) )
			.Build();

		new Upgrade.Builder( "Village Size I", "Increase your village size." )
			.ConfigureWith( v =>
			{
				v.VillageSize = 25f; // +25
			} )
			.WithTexture( "expansion/expansion1.png" )
			.PlaceAt( Vector2.Down * 450 )
			.Next( "Village Size II",
				v =>
					v.WithTexture( "expansion/expansion2.png" )
						.PlaceAt( Vector2.Down * 450 + Vector2.Left * 150 ) )
			.Next( "Village Size III",
				v =>
					v.WithTexture( "expansion/expansion3.png" )
						.PlaceAt( Vector2.Down * 450 + Vector2.Left * 300 ) )
			.Next( "Village Size IV",
				v =>
					v.WithTexture( "expansion/expansion4.png" )
						.PlaceAt( Vector2.Down * 450 + Vector2.Left * 450 ) )
			.Next( "Village Size V",
				v =>
					v.WithTexture( "expansion/expansion5.png" )
						.PlaceAt( Vector2.Down * 450 + Vector2.Left * 600 ) )
			.Build();

		new Upgrade.Builder( "Recovery Training I", "Increase your Energy recharge rate." )
			.ConfigureWith( v =>
			{
				v.RecoveryTraining = 1f; // +50%
			} )
			.WithTexture( "recovery/recovery1.png" )
			.PlaceAt( Vector2.Down * 600 )
			.Next( "Recovery Training II",
				v =>
					v.WithTexture( "recovery/recovery2.png" )
						.PlaceAt( Vector2.Down * 600 + Vector2.Left * 150 ) )
			.Next( "Recovery Training III",
				v =>
					v.WithTexture( "recovery/recovery3.png" )
						.PlaceAt( Vector2.Down * 600 + Vector2.Left * 300 ) )
			.Next( "Recovery Training IV",
				v =>
					v.WithTexture( "recovery/recovery4.png" )
						.PlaceAt( Vector2.Down * 600 + Vector2.Left * 450 ) )
			.Next( "Recovery Training V",
				v =>
					v.WithTexture( "recovery/recovery5.png" )
						.PlaceAt( Vector2.Down * 600 + Vector2.Left * 600 ) )
			.Build();

		new Upgrade.Builder( "Endurance Training I", "Increase your max Energy." )
			.ConfigureWith( v =>
			{
				v.EnduranceTraining = 1f; // +50%
			} )
			.WithTexture( "endurance/endurance1.png" )
			.PlaceAt( Vector2.Down * 750 )
			.Next( "Endurance Training II",
				v =>
					v.WithTexture( "endurance/endurance2.png" )
						.PlaceAt( Vector2.Down * 750 + Vector2.Left * 150 ) )
			.Next( "Endurance Training III",
				v =>
					v.WithTexture( "endurance/endurance3.png" )
						.PlaceAt( Vector2.Down * 750 + Vector2.Left * 300 ) )
			.Next( "Endurance Training IV",
				v =>
					v.WithTexture( "endurance/endurance4.png" )
						.PlaceAt( Vector2.Down * 750 + Vector2.Left * 450 ) )
			.Next( "Endurance Training V",
				v =>
					v.WithTexture( "endurance/endurance5.png" )
						.PlaceAt( Vector2.Down * 750 + Vector2.Left * 600 ) )
			.Build();


		new Upgrade.Builder( "Swiftness I", "Increase your walking speed." )
			.ConfigureWith( v =>
			{
				v.Swiftness = 0.1f; // +10%
			} )
			.WithTexture( "swiftness/swiftness1.png" )
			.PlaceAt( Vector2.Down * 900 )
			.Next( "Swiftness II",
				v =>
					v.WithTexture( "swiftness/swiftness2.png" )
						.PlaceAt( Vector2.Down * 900 + Vector2.Left * 150 ) )
			.Next( "Swiftness III",
				v =>
					v.WithTexture( "swiftness/swiftness3.png" )
						.PlaceAt( Vector2.Down * 900 + Vector2.Left * 300 ) )
			.Build();

		new Upgrade.Builder( "Weapons I", "Workbenches create better and stronger weapons" )
			.ConfigureWith( v =>
			{
				v.Weapons = 1f; // Next level
			} )
			.WithTexture( "weapons/weapons1.png" )
			.PlaceAt( Vector2.Down * 1050 )
			.Next( "Weapons II",
				v =>
					v.WithTexture( "weapons/weapons2.png" )
						.PlaceAt( Vector2.Down * 1050 + Vector2.Left * 150 ) )
			.Next( "Weapons III",
				v =>
					v.WithTexture( "weapons/weapons3.png" )
						.PlaceAt( Vector2.Down * 1050 + Vector2.Left * 300 ) )
			.Next( "Weapons IV",
				v =>
					v.WithTexture( "weapons/weapons4.png" )
						.PlaceAt( Vector2.Down * 1050 + Vector2.Left * 450 ) )
			.Next( "Weapons V",
				v =>
					v.WithTexture( "weapons/weapons5.png" )
						.PlaceAt( Vector2.Down * 1050 + Vector2.Left * 600 ) )
			.Build();


		new Upgrade.Builder( "Fortitude I", "Increase your health." )
			.ConfigureWith( v =>
			{
				v.Fortitude = 0.25f; // +25%
			} )
			.WithTexture( "fortitude/fortitude1.png" )
			.PlaceAt( Vector2.Down * 1200 )
			.Next( "Fortitude II",
				v =>
					v.WithTexture( "fortitude/fortitude2.png" )
						.PlaceAt( Vector2.Down * 1200 + Vector2.Left * 150 ) )
			.Next( "Fortitude III",
				v =>
					v.WithTexture( "fortitude/fortitude3.png" )
						.PlaceAt( Vector2.Down * 1200 + Vector2.Left * 300 ) )
			.Next( "Fortitude IV",
				v =>
					v.WithTexture( "fortitude/fortitude4.png" )
						.PlaceAt( Vector2.Down * 1200 + Vector2.Left * 450 ) )
			.Build();

		new Upgrade.Builder( "Power Milk I", "Your goblins are born stronger." )
			.ConfigureWith( v =>
			{
				v.Milk = 1f; // +1 level
			} )
			.WithTexture( "milk/milk1.png" )
			.PlaceAt( Vector2.Down * 1350 )
			.Next( "Power Milk II",
				v =>
					v.WithTexture( "milk/milk2.png" )
						.PlaceAt( Vector2.Down * 1350 + Vector2.Left * 150 ) )
			.Next( "Power Milk III",
				v =>
					v.WithTexture( "milk/milk3.png" )
						.PlaceAt( Vector2.Down * 1350 + Vector2.Left * 300 ) )
			.Build();

		new Upgrade.Builder( "Backseat Gaming I", "Allies around you attack faster." )
			.ConfigureWith( v =>
			{
				v.BackseatGaming = 0.20f; // -20%
			} )
			.WithTexture( "backseat/backseat1.png" )
			.PlaceAt( Vector2.Down * 1500 )
			.Next( "Backseat Gaming II",
				v =>
					v.WithTexture( "backseat/backseat2.png" )
						.PlaceAt( Vector2.Down * 1500 + Vector2.Left * 150 ) )
			.Next( "Backseat Gaming III",
				v =>
					v.WithTexture( "backseat/backseat3.png" )
						.PlaceAt( Vector2.Down * 1500 + Vector2.Left * 300 ) )
			.Build();


		new Upgrade.Builder( "Nature's Call I", "Allies move faster while retreating to the forest." )
			.ConfigureWith( v =>
			{
				v.NatureCalls = 0.15f; // +15%
			} )
			.WithTexture( "nature/nature1.png" )
			.PlaceAt( Vector2.Down * 1650 )
			.Next( "Nature's Call II",
				v =>
					v.WithTexture( "nature/nature2.png" )
						.PlaceAt( Vector2.Down * 1650 + Vector2.Left * 150 ) )
			.Next( "Nature's Call III",
				v =>
					v.WithTexture( "nature/nature3.png" )
						.PlaceAt( Vector2.Down * 1650 + Vector2.Left * 300 ) )
			.Build();

		new Upgrade.Builder( "Mac Mansions I", "Tents store 1 extra goblin" )
			.ConfigureWith( v =>
			{
				v.MacMansion = 1f; // +1 unit
			} )
			.WithTexture( "mansion/mansion1.png" )
			.PlaceAt( Vector2.Down * 1800 )
			.Next( "Mac Mansions II",
				v =>
					v.WithTexture( "mansion/mansion2.png" )
						.PlaceAt( Vector2.Down * 1800 + Vector2.Left * 150 ) )
			.Next( "Mac Mansions III",
				v =>
					v.WithTexture( "mansion/mansion3.png" )
						.PlaceAt( Vector2.Down * 1800 + Vector2.Left * 300 ) )
			.Next( "Mac Mansions IV",
				v =>
					v.WithTexture( "mansion/mansion4.png" )
						.PlaceAt( Vector2.Down * 1800 + Vector2.Left * 450 ) )
			.Next( "Mac Mansions V",
				v =>
					v.WithTexture( "mansion/mansion4.png" )
						.PlaceAt( Vector2.Down * 1800 + Vector2.Left * 600 ) )
			.Build();
	}
}
