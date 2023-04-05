using GameJam.UpgradeSystem;
using Sandbox.UI;

namespace GameJam.UI;

public partial class SkillTree
{
	private void GenerateUpgrades( Panel panel )
	{
		foreach ( var upgrade in Upgrade.All )
		{
			var actor = new UpgradeActor( Camera, upgrade );
			panel.AddChild( actor );
		}

		var actors = Descendants.OfType<UpgradeActor>().ToList();

		foreach ( var actor in actors )
		{
			foreach ( var dependencyId in actor.Upgrade.Dependencies )
			{
				var dependencyActor = actors.SingleOrDefault( v => v.Upgrade.Identifier == dependencyId );
				if ( dependencyActor == null )
					throw new Exception( $"Unknown or no existing actor for dependency {dependencyId}" );
				actor.Dependencies.Add( dependencyActor );
			}
		}

		// Set initial selected actor
		Select( actors.FirstOrDefault() );
	}

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

		ClearSelection();

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
			Camera.Position += (MousePosition - _lastMousePos) * ScaleFromScreen;
		}

		_lastMousePos = MousePosition;
	}
}
