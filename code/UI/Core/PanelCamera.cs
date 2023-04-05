namespace GameJam.UI.Core;

public class PanelCamera
{
	private Vector2 _position = Vector2.Zero;

	public Rect? Bounds;

	public Vector2 Position
	{
		get => _position;
		set
		{
			_position = value;
			if ( Bounds.HasValue )
			{
				var bounds = Bounds.Value;
				if ( _position.x < bounds.Left )
					_position.x = bounds.Left;
				if ( _position.x > bounds.Right )
					_position.x = bounds.Right;

				if ( _position.y < bounds.Top )
					_position.y = bounds.Top;
				if ( _position.y > bounds.Bottom )
					_position.y = bounds.Bottom;
			}

			PropagateUpdate();
		}
	}

	public Vector2 Negative => -Position;

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
