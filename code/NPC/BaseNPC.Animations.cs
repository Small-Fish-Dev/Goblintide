namespace GameJam;

public partial class BaseNPC
{
	public virtual void ComputeAnimations()
	{
		if ( Velocity.LengthSquared > 100 )
			Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( Velocity.WithZ( 0f ), Vector3.Up ), Time.Delta * 6f );

		SetAnimParameter( "move_x", Velocity.Dot( Rotation.Forward ) / Scale );
		SetAnimParameter( "move_y", Velocity.Dot( Rotation.Forward ) / Scale );
	}
}
