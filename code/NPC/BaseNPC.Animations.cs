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
				Rotation = Rotation.LookAt( CurrentTarget.Position - Position );
			else if ( CurrentSubBehaviour == SubBehaviour.Panicking )
				Rotation = Rotation.LookAt( Position - CurrentTarget.Position );
		}

		SetAnimParameter( "move_x", Velocity.Dot( Rotation.Forward ) / Scale );
		SetAnimParameter( "move_y", Velocity.Dot( Rotation.Right ) / Scale );

		if ( CurrentSubBehaviour == SubBehaviour.Attacking )
			SetAnimParameter( "holdtype", 5 );
		else
		{
			SetAnimParameter( "holdtype", 0 );
		}

		if ( CurrentSubBehaviour == SubBehaviour.Panicking )
		{
			// TODO: Proper panic when grod makes custom animgraph
			SetAnimParameter( "duck", 0.3f );
			SetAnimParameter( "holdtype", 5 );
			SetAnimParameter( "holdtype_pose", 1.8f );
		}
		else
		{
			SetAnimParameter( "duck", 0f );
			SetAnimParameter( "holdtype", 0 );
			SetAnimParameter( "holdtype_pose", 0f );
		}
	}
}
