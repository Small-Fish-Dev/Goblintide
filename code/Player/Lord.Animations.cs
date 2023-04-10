namespace GoblinGame;

public partial class Lord
{
	private void PointHead( Rotation rotation )
	{
		var angles = rotation.Angles();
		angles.yaw = 0;
		rotation = angles.ToRotation();
		var direction = rotation.Forward;
		SetAnimParameter( "aim_head", direction );
	}

	public void SimulateAnimations()
	{
		// note(gio): made the animations a bit more speedy - nicer to look at imo
		SetAnimParameter( "move_x", Velocity.Dot( Rotation.Forward ) * 1.35f );
		SetAnimParameter( "move_y", Velocity.Dot( Rotation.Right ) * 1.35f );

		if ( Pointing )
		{
			PointHead( LookDirection );
		}
		else
		{
			PointHead( Rotation.Identity );
		}
	}
}
