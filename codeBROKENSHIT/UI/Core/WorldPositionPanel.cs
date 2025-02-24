using Sandbox.UI;

public class WorldPositionPanel : Panel
{
	public Vector3 Position;

	protected virtual void UpdatePos()
	{
		Style.Position = PositionMode.Absolute;

		var position = (Vector2)Position.ToScreen();

		position *= Screen.Size;

		position -= Box.Rect.Size / 2;

		position *= ScaleFromScreen;

		Style.Left = position.x;
		Style.Top = position.y;
	}

	public override void Tick()
	{
		base.Tick();

		UpdatePos();

		StateHasChanged();
	}
}
