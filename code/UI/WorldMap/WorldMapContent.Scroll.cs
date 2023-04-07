using GameJam.UI.Core;
using Sandbox.UI;

namespace GameJam.UI;

public partial class WorldMapContent
{
	private bool _dragging;
	private Vector2 _lastMousePos;

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

		if ( _dragging )
		{
			PanelCamera.Position -= (MousePosition - _lastMousePos) * ScaleFromScreen;
		}

		_lastMousePos = MousePosition;
	}
}
