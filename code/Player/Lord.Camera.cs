namespace GameJam;

public partial class Lord
{
	public void SimulateCamera()
	{
		Rotation = Rotation.FromYaw( ViewAngles.yaw );
		Camera.Position = Position + Vector3.Up * 40f + Rotation.Backward * 80f + Rotation.Up * 50f;
		Camera.Rotation = Rotation * Rotation.FromPitch( 20f );

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
	}
}
