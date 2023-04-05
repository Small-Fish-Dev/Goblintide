using Sandbox.UI;

namespace GameJam.UI;

public partial class WorldMap
{
	private Panel Container { get; set; }
	private Vector2 offset { get; set; }
	private Vector2 position { get; set; }

	public IEnumerable<Actor> Actors => Descendants.OfType<Actor>();

	private bool _dragging;

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime ) return;

		foreach ( var entry in WorldMapHost.Entries )
		{
			if ( entry is WorldMapHost.Generator generator )
			{
				var panel = new GeneratorActor( generator );
				Container.AddChild( panel );
				var size = 300f * (float)Math.Sqrt( generator.Size / 5 );

				panel.Style.SetBackgroundImage( "ui/camp.png" );
				if ( size > 1200f )
					panel.Style.SetBackgroundImage( "ui/town.png" );
				if ( size > 2500f )
					panel.Style.SetBackgroundImage( "ui/castle.png" );
			}
		}
	}

	/// <summary> Whether or not the world map can be translated </summary>
	private static bool CanStartDragging( Panel target )
	{
		var attribute = target.GetAttribute( "allow-translate" );
		return attribute != null;
	}

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		if ( !CanStartDragging( e.Target ) ) return;

		if ( e.MouseButton != MouseButtons.Left ) return;

		foreach ( var actor in Actors )
			actor.Offset = actor.Position - MousePosition * ScaleFromScreen;

		offset = position - MousePosition;

		_dragging = true;
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		_dragging = false;
	}

	protected override void OnMouseMove( MousePanelEvent e )
	{
		base.OnMouseMove( e );

		if ( !_dragging )
			return;

		foreach ( var actor in Actors )
			actor.Position = actor.Offset + MousePosition  * ScaleFromScreen;

		Log.Info( position );
		position = offset + MousePosition;
		Container.Style.BackgroundPositionX = position.x;
		Container.Style.BackgroundPositionY = position.y;
	}
}
