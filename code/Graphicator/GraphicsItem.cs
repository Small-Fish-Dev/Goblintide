namespace Graphicator;

public abstract class GraphicsItem
{
	public Point Position;
	public Point Size;

	public Rect ScreenspaceRect => new(Position, Size);

	public abstract void Render();
	public virtual void Update() { }
}
