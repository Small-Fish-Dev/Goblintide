namespace GameJam.UpgradeSystem;

public static class Creator
{
	static Creator()
	{
		new Upgrade.Builder( "Aura of Fear I" )
			.ConfigureWith( v => v.DecreasedAreaDiligence = 0.04f )
			.Next( "Aura of Fear II" )
			.Next( "Aura of Fear III" )
			.Next( "Aura of Fear IV" )
			.Build();
	}
}
