using Sandbox.UI;

namespace GameJam.UI;

public class PlayerView : ScenePanel
{
	public SceneModel Model { get; set; }

	private bool _animated = false;
	private readonly TimeUntil _animate = 0.4f;

	public PlayerView()
	{
		World = new SceneWorld();

		Model = new SceneModel( World, Lord.Self.Model, Lord.Self.Transform );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible ) return;

		if ( _animate && !_animated )
		{
			Model.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.HoldItem );
			Model.SetAnimParameter( "holdtype_handedness", (int)CitizenAnimationHelper.Hand.Both );
			Model.SetAnimParameter( "holdtype_pose", 4.75f );
			_animated = true;
		}

		if ( !_animated )
		{
			var main = Sandbox.Camera.Main;

			var playerOffset = Vector3.Down * 20;
			var cameraOffset = Vector3.Up * 20;
			var distance = 130;
			
			var a = Lord.Self.EyePosition;
			a += Model.Rotation.Forward * distance;
	
			var b = Lord.Self.EyePosition;
			b += playerOffset;
	
			Camera.Position = a;
			Camera.Rotation = Rotation.LookAt( b - a );
			Camera.FieldOfView = 20;
			Camera.AntiAliasing = true;
		}

		Model.CurrentSequence.Time = Lord.Self.CurrentSequence.Time;
		Model.Rotation = Lord.Self.Rotation;
		Model.Update( Time.Delta );
	}
}
