namespace GameJam;

public partial class BaseNPC
{
	public virtual void ComputeAnimations()
	{
		if ( Velocity.LengthSquared > 100 )
			Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( Velocity.WithZ( 0f ), Vector3.Up ), Time.Delta * 6f );

		if ( CurrentTarget.IsValid() )
		{
			if ( CurrentSubBehaviour == SubBehaviour.Attacking )
				Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( (CurrentTarget.Position - Position).WithZ( 0f ) ), Time.Delta * 6f );
			else if ( CurrentSubBehaviour == SubBehaviour.Panicking )
				Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( (Position - CurrentTarget.Position).WithZ( 0f ) ), Time.Delta * 6f );
		}

		SetAnimParameter( "move_x", Velocity.Dot( Rotation.Forward ) / Scale );
		SetAnimParameter( "move_y", Velocity.Dot( Rotation.Right ) / Scale );

		if ( CurrentSubBehaviour == SubBehaviour.Attacking )
		{
			SetAnimParameter( "State", 1 );
		}
		else if ( CurrentSubBehaviour == SubBehaviour.Panicking )
		{
			SetAnimParameter( "State", 2 );
		}
		else
		{
			SetAnimParameter( "State", 0 );
		}
	}
}
