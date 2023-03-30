using Sandbox.UI;

namespace GameJam.UI;

public class PlayerView : ScenePanel
{
	public SceneModel Model { get; set; }

	private Lord Pawn => (Lord)Game.LocalPawn;

	private bool _animated = false;
	private readonly TimeUntil _animate = 0.4f;
	
	public PlayerView()
	{
		World = new SceneWorld();

		Model = new SceneModel( World, Pawn.Model, Pawn.Transform );
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
		
		Model.CurrentSequence.Time = Pawn.CurrentSequence.Time;
		Model.Update( Time.Delta );

		var main = Sandbox.Camera.Main;

		Camera.Position = main.Position;
		Camera.Rotation = main.Rotation;
		Camera.FieldOfView = main.FieldOfView;
		Camera.AntiAliasing = true;
	}
}
