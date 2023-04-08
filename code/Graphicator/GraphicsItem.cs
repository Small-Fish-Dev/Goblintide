namespace Graphicator;

public abstract class GraphicsItem
{
	public Point Position;
	public Point Size;

	public abstract float Opacity { get; set; }

	public Rect ScreenspaceRect => new(Position, Size);

	public abstract void Render();
	public virtual void Update() { }
}
