using Sandbox.UI;

namespace GameJam.UI;

public class PlayerView : ScenePanel
{
	public SceneModel Model { get; set; }

	public PlayerView()
	{
		World = new SceneWorld();
		var pawn = (Lord)Game.LocalPawn;

		Model = new SceneModel( World, pawn.Model, pawn.Transform );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible ) return;

		Model.Update( Time.Delta );

		Camera.Position = Sandbox.Camera.Main.Position;
		Camera.Rotation = Sandbox.Camera.Main.Rotation;
	}
}
