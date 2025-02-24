using Graphicator.Internal;
using Sandbox.UI;

namespace Graphicator.PanelSupport;

/// <summary>
/// Image with support for AnimationPack animations
/// </summary>
public class AnimatedImage : Image
{
	private readonly Stack<AnimationPlayback> _stack = new();

	public override void Tick()
	{
		base.Tick();

		if ( !_stack.TryPeek( out var top ) ) return;

		top.Update();
	}

	private void OnPlaybackComplete( object sender, EventArgs e )
	{
		var playback = (AnimationPlayback)sender;
		if ( playback.ShouldRepeat ) return;
		if ( sender == _stack.Peek() )
			_stack.Pop();
	}

	private void OnFrameChanged( object sender, AnimationPlayback.FrameChangedEventArgs e )
	{
		if ( sender == _stack.Peek() )
			Texture = e.Frame.Texture;
	}

	public AnimationPlayback Push( Animation animation )
	{
		if ( animation == null )
			throw new Exception( "Provided animation was null" );
		var playback = new AnimationPlayback( animation );
		playback.AnimationCompleted += OnPlaybackComplete;
		playback.FrameChanged += OnFrameChanged;
		_stack.Push( playback );
		return playback;
	}

	public AnimationPlayback Push( string path ) => Push( Animation.Find( path ) );
}
