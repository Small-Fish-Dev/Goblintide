using Sandbox.UI;

namespace GameJam.UI;

public partial class WorldMap
{
	private Panel Container { get; set; }

	public IEnumerable<Actor> Actors => Descendants.OfType<Actor>();

	private bool _dragging;

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime ) return;

		foreach ( var entry in WorldMapHost.Entries )
		{
			if ( entry is WorldMapHost.Generator generator )
				Container.AddChild( new GeneratorActor( generator ) );
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
			actor.Offset = actor.Position - MousePosition;

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
			actor.Position = actor.Offset + MousePosition;
	}
}
