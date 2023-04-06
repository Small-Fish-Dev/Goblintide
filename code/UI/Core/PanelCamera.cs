namespace GameJam.UI.Core;

public class PanelCamera
{
	private Vector2 _position = Vector2.Zero;

	public Rect? Bounds;

	private struct Animation
	{
		public float EndTime;
		public float StartTime;
		public Vector2 From;
		public Vector2 To;
		public Func<float, float> EasingFunction;
	}

	public void Tick()
	{
		_animations.RemoveAll( v => Time.Now > v.EndTime );
		foreach ( var animation in _animations )
		{
			var f = Time.Now.Remap( animation.StartTime, animation.EndTime );
			var e = animation.EasingFunction( f );
			Position = Vector2.Lerp( animation.From, animation.To, e );
		}
	}

	private readonly List<Animation> _animations = new();

	public void AddAnimation( float length, Vector2 end, string easing = "ease-in-out" )
	{
		var startTime = Time.Now;

		if ( _animations.Count != 0 )
		{
			var lastAnimation = _animations.Last();
			startTime = lastAnimation.EndTime;
		}

		var animation = new Animation
		{
			From = Position, To = end, EndTime = startTime + length, StartTime = startTime
		};

		var fn = Easing.GetFunction( easing );
		animation.EasingFunction = v => fn( v );

		_animations.Add( animation );
	}

	public void NewAnimation( float length, Vector2 end, string easing = "ease-in-out" )
	{
		_animations.Clear();

		var animation = new Animation
		{
			From = Position, To = end, EndTime = Time.Now + length, StartTime = Time.Now
		};

		var fn = Easing.GetFunction( easing );
		animation.EasingFunction = v => fn( v );

		_animations.Add( animation );
	}

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

	public Vector2 GetActorCameraCenter( Actor actor )
	{
		var cam = actor.Rect.Center;
		var screen = Screen.Size;
		screen.x *= 0.7f;
		cam -= screen / 2;
		return cam;
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
