namespace GameJam.UpgradeSystem;

public static class Creator
{
	static Creator() => Build();

	[Event.Hotload]
	private static void Build()
	{
		Upgrade.ClearAll();
		new Upgrade.Builder( "Aura of Fear I" )
			.ConfigureWith( v =>
			{
				v.DecreasedAreaDiligence = 0.04f;
			} )
			.Next( "Aura of Fear II",
				v => v.PlaceAt( Vector2.Down * 128 ).WithCost( 25 ) )
			.Next( "Aura of Fear III",
				v => v.PlaceAt( Vector2.Down * 256 ) )
			.Next( "Aura of Fear IV",
				v => v.PlaceAt( Vector2.Down * 400 ) )
			.Build();
	}
}
