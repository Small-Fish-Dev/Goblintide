namespace Graphicator.Internal;

public class AnimationPlayback
{
	public class FrameChangedEventArgs : EventArgs
	{
		public AnimationFrame Frame;
	}

	public event EventHandler<FrameChangedEventArgs> FrameChanged;
	public event EventHandler AnimationCompleted;

	public bool ShouldRepeat;

	private readonly WeakReference _ref;
	private int _index;
	private TimeUntil _next;
	private float _delay = 1;

	public bool HasCompleted => !_ref.IsAlive || _index >= ((Animation)_ref.Target)!.Entries.Count;

	private IAnimationEntry Next() => !_ref.IsAlive ? null : ((Animation)_ref.Target)!.Entries[_index++];

	public AnimationPlayback( Animation animation )
	{
		_ref = new WeakReference( animation );
		animation.ProcessingInstanceSwap += HandleInstanceSwap;
	}

	public void Update()
	{
		if ( !_next ) return;
		HandleNext();
		_next = _delay;
	}

	private void HandleInstanceSwap( object sender, Animation.InstanceSwapEventArgs e )
	{
		var prev = (Animation)sender;
		prev.ProcessingInstanceSwap -= HandleInstanceSwap;
		_ref.Target = e.Next;
		Log.Info( $"Handling instance swap! {e.Next}" );
		Reset();
		HandleNext();
		e.Next.ProcessingInstanceSwap += HandleInstanceSwap;
	}

	private void HandleNext()
	{
		if ( !_ref.IsAlive )
		{
			Log.Warning( "HandleNext called with broken reference" );
			return;
		}

		var animation = (Animation)_ref.Target;
		if ( HasCompleted )
		{
			AnimationCompleted?.Invoke( this, EventArgs.Empty );
			if ( ShouldRepeat )
			{
				Reset();
				return;
			}

			AnimationCompleted = null;
			return;
		}

		while ( !HasCompleted )
		{
			var part = Next();
			switch ( part )
			{
				case AnimationFrame frame:
					FrameChanged?.Invoke( this, new FrameChangedEventArgs { Frame = frame } );
					return; // important - animation frame should stop the loop
				case AnimationControlFrame ctrl:
					_delay = ctrl.NewDelay ?? _delay;
					break;
			}
		}
	}

	public void Reset() => _index = 0;
}
