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
				v => v.WithCost( 25 ) )
			.Next( "Aura of Fear III" )
			.Next( "Aura of Fear IV" )
			.Build();
	}
}
