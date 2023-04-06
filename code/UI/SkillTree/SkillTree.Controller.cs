using GameJam.UpgradeSystem;
using Sandbox.UI;

namespace GameJam.UI;

public partial class SkillTree
{
	private Actor Trace( Vector2 point, Vector2 direction, float distance, float cover, Func<Actor, bool> check )
	{
		var position = point;
		var normal = direction.Normal;

		while ( point.Distance( position ) < distance )
		{
			position += normal * cover;

			foreach ( var actor in Descendants.OfType<Actor>() )
			{
				if ( !check( actor ) ) continue;
				if ( actor.Rect.IsInside( position ) )
					return actor;
			}
		}

		return null;
	}

	protected override void OnInputEvent( InputEvent e )
	{
		base.OnInputEvent( e );

		if ( !e.Pressed ) return;

		Vector2 direction;
		switch ( e.Action )
		{
			case Action.Up:
				direction = new Vector2( 0, -1 );
				break;
			case Action.Down:
				direction = new Vector2( 0, 1 );
				break;
			case Action.Left:
				direction = new Vector2( -1, 0 );
				break;
			case Action.Right:
				direction = new Vector2( 1, 0 );
				break;
			default:
				return;
		}

		if ( SelectedActor == null )
		{
			SelectedActor = Descendants.OfType<UpgradeActor>().First();
			return;
		}

		var result = Trace( SelectedActor.Position + 5, direction, 512, 16, actor =>
		{
			if ( actor == SelectedActor )
				return false;
			if ( actor is not UpgradeActor )
				return false;
			return true;
		} );
		if ( result == null )
			return;
		var actor = (UpgradeActor)result;
		Select( actor );

		Camera.NewAnimation( 0.2f, Camera.GetActorCameraCenter( actor ) );
	}
}
