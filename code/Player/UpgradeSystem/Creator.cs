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
			.WithTexture( "aura/aura1.png" )
			.Next( "Aura of Fear II",
				v =>
					v.WithTexture( "aura/aura2.png" )
					.PlaceAt( Vector2.Left * 150 ) )
			.Next( "Aura of Fear III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Left * 300 ) )
			.Next( "Aura of Fear IV",
				v =>
					v.WithTexture( "aura/aura4.png" )
						.PlaceAt( Vector2.Left * 450 ) )
			.Next( "Aura of Fear V",
				v =>
					v.WithTexture( "aura/aura5.png" )
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
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 300 )
			.Next( "Goblin School II",
				v =>
					v.WithTexture( "aura/aura2.png" )
						.PlaceAt( Vector2.Down * 300 + Vector2.Left * 150 ) )
			.Next( "Goblin School III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 300 + Vector2.Left * 300 ) )
			.Next( "Goblin School IV",
				v =>
					v.WithTexture( "aura/aura4.png" )
						.PlaceAt( Vector2.Down * 300 + Vector2.Left * 450 ) )
			.Next( "Goblin School V",
				v =>
					v.WithTexture( "aura/aura5.png" )
						.PlaceAt( Vector2.Down * 300 + Vector2.Left * 600 ) )
			.Build();

		new Upgrade.Builder( "Village Size I", "Increase your village size." )
			.ConfigureWith( v =>
			{
				v.VillageSize = 25f; // +25
			} )
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 450 )
			.Next( "Village Size II",
				v =>
					v.WithTexture( "aura/aura2.png" )
						.PlaceAt( Vector2.Down * 450 + Vector2.Left * 150 ) )
			.Next( "Village Size III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 450 + Vector2.Left * 300 ) )
			.Next( "Village Size IV",
				v =>
					v.WithTexture( "aura/aura4.png" )
						.PlaceAt( Vector2.Down * 450 + Vector2.Left * 450 ) )
			.Next( "Village Size V",
				v =>
					v.WithTexture( "aura/aura5.png" )
						.PlaceAt( Vector2.Down * 450 + Vector2.Left * 600 ) )
			.Build();

		new Upgrade.Builder( "Recovery Training I", "Increase your Energy recharge rate." )
			.ConfigureWith( v =>
			{
				v.RecoveryTraining = 1f; // +50%
			} )
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 600 )
			.Next( "Recovery Training II",
				v =>
					v.WithTexture( "aura/aura2.png" )
						.PlaceAt( Vector2.Down * 600 + Vector2.Left * 150 ) )
			.Next( "Recovery Training III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 600 + Vector2.Left * 300 ) )
			.Next( "Recovery Training IV",
				v =>
					v.WithTexture( "aura/aura4.png" )
						.PlaceAt( Vector2.Down * 600 + Vector2.Left * 450 ) )
			.Next( "Recovery Training V",
				v =>
					v.WithTexture( "aura/aura5.png" )
						.PlaceAt( Vector2.Down * 600 + Vector2.Left * 600 ) )
			.Build();

		new Upgrade.Builder( "Endurance Training I", "Increase your max Energy." )
			.ConfigureWith( v =>
			{
				v.EnduranceTraining = 1f; // +50%
			} )
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 750 )
			.Next( "Endurance Training II",
				v =>
					v.WithTexture( "aura/aura2.png" )
						.PlaceAt( Vector2.Down * 750 + Vector2.Left * 150 ) )
			.Next( "Endurance Training III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 750 + Vector2.Left * 300 ) )
			.Next( "Endurance Training IV",
				v =>
					v.WithTexture( "aura/aura4.png" )
						.PlaceAt( Vector2.Down * 750 + Vector2.Left * 450 ) )
			.Next( "Endurance Training V",
				v =>
					v.WithTexture( "aura/aura5.png" )
						.PlaceAt( Vector2.Down * 750 + Vector2.Left * 600 ) )
			.Build();


		new Upgrade.Builder( "Swiftness I", "Increase your walking speed." )
			.ConfigureWith( v =>
			{
				v.Swiftness = 0.1f; // +10%
			} )
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 900 )
			.Next( "Swiftness II",
				v =>
					v.WithTexture( "aura/aura2.png" )
						.PlaceAt( Vector2.Down * 900 + Vector2.Left * 150 ) )
			.Next( "Swiftness III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 900 + Vector2.Left * 300 ) )
			.Build();

		new Upgrade.Builder( "Sticky Fingers I", "More resources collected during the raid." )
			.ConfigureWith( v =>
			{
				v.StickyFingers = 0.15f; // +15%
			} )
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 1050 )
			.Next( "Sticky Fingers II",
				v =>
					v.WithTexture( "aura/aura2.png" )
						.PlaceAt( Vector2.Down * 1050 + Vector2.Left * 150 ) )
			.Next( "Sticky Fingers III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 1050 + Vector2.Left * 300 ) )
			.Build();


		new Upgrade.Builder( "Fortitude I", "Increase your health." )
			.ConfigureWith( v =>
			{
				v.Fortitude = 0.25f; // +25%
			} )
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 1200 )
			.Next( "Fortitude II",
				v =>
					v.WithTexture( "aura/aura2.png" )
						.PlaceAt( Vector2.Down * 1200 + Vector2.Left * 150 ) )
			.Next( "Fortitude III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 1200 + Vector2.Left * 300 ) )
			.Next( "Fortitude IV",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 1200 + Vector2.Left * 450 ) )
			.Build();


		new Upgrade.Builder( "Gumshoes I", "Decrease chances of being targetted." )
			.ConfigureWith( v =>
			{
				v.Gumshoes = 0.25f; // +25%
			} )
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 1350 )
			.Next( "Gumshoes II",
				v =>
					v.WithTexture( "aura/aura2.png" )
						.PlaceAt( Vector2.Down * 1350 + Vector2.Left * 150 ) )
			.Next( "Gumshoes III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 1350 + Vector2.Left * 300 ) )
			.Build();


		new Upgrade.Builder( "Backseat Gaming I", "Allies around you attack faster." )
			.ConfigureWith( v =>
			{
				v.BackseatGaming = 0.20f; // -20%
			} )
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 1500 )
			.Next( "Backseat Gaming II",
				v =>
					v.WithTexture( "aura/aura2.png" )
						.PlaceAt( Vector2.Down * 1500 + Vector2.Left * 150 ) )
			.Next( "Backseat Gaming III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 1500 + Vector2.Left * 300 ) )
			.Build();


		new Upgrade.Builder( "Nature's Call I", "Allies move faster while retreating to the forest." )
			.ConfigureWith( v =>
			{
				v.NatureCalls = 0.15f; // +15%
			} )
			.WithTexture( "aura/aura1.png" )
			.PlaceAt( Vector2.Down * 1650 )
			.Next( "Nature's Call II",
				v =>
					v.WithTexture( "aura/aura2.png" )
						.PlaceAt( Vector2.Down * 1650 + Vector2.Left * 150 ) )
			.Next( "Nature's Call III",
				v =>
					v.WithTexture( "aura/aura3.png" )
						.PlaceAt( Vector2.Down * 1650 + Vector2.Left * 300 ) )
			.Build();
	}
}
