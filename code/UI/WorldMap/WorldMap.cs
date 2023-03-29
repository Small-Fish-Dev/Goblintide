using Sandbox.UI;

namespace GameJam.UI;

public partial class WorldMap
{
	private Panel Container { get; set; }

	public IEnumerable<MapActor> Actors => Descendants.OfType<MapActor>();

	private bool _dragging;

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
			actor.DragOffset = actor.Position - MousePosition * ScaleFromScreen;

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
			actor.Position = actor.DragOffset + MousePosition * ScaleFromScreen;
	}
}
