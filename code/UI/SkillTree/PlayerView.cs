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
			Camera.Position = main.Position;
			Camera.Rotation = main.Rotation;
			Camera.FieldOfView = main.FieldOfView;
			Camera.AntiAliasing = true;
		}
		
		Model.CurrentSequence.Time = Lord.Self.CurrentSequence.Time;
		Model.Rotation = Lord.Self.Rotation;
		Model.Update( Time.Delta );
	}
}
