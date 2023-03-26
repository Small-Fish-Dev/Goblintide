namespace GameJam;

public partial class Lord
{
	public void SimulateAnimations()
	{
		SetAnimParameter( "move_x", Velocity.Dot( Rotation.Forward ) );
		SetAnimParameter( "move_y", Velocity.Dot( Rotation.Right ) );
	}
}
