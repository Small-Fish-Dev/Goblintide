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
			.WithTexture( "aura/aura1.png" )
			.Next( "Aura of Fear II",
				v =>
					v.WithCost( 25 )
						.WithTexture( "aura/aura2.png" )
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
			.Next( "Aura of Fear VI",
				v =>
					v.WithTexture( "aura/aura6.png" )
						.PlaceAt( Vector2.Left * 750 ) )
			.Build();
	}
}
