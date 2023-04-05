namespace GameJam.UI.Core;

public class PanelCamera
{
	private Vector2 _position = Vector2.Zero;

	public Vector2 Position
	{
		get => _position;
		set
		{
			_position = value;
			PropagateUpdate();
		}
	}

	private void PropagateUpdate()
	{
		foreach ( var r in _refs.Where( r => r.IsAlive ) )
		{
			((Actor)r.Target)?.PositionHasChanged();
		}
	}

	private readonly List<WeakReference> _refs = new();

	public void Register( Actor actor ) => _refs.Add( new WeakReference( actor ) );
	public void Unregister( Actor actor ) => _refs.RemoveAll( v => v.Target == actor );
}
