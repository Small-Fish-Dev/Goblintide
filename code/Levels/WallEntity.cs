using Sandbox.Diagnostics;

namespace GameJam;


public partial class WallEntity : Entity
{
	internal SceneObject sceneObject { get; set; }
	public float Length { get; set; } = 100f;
	public static BBox Box { get; set; } = new BBox( new Vector3( -10f ), new Vector3( 10f ) );

	public WallEntity( SceneWorld sceneWorld, String model, Transform transform, float length )
	{
		sceneObject = new SceneObject(sceneWorld, model, transform);
		Transform = transform;
		Length = length;
	}

	[Event.Client.Frame]
	public void TryCollision()
	{
		if ( Time.Tick % 5 == 0 )
		{
			var trace = Trace.Box( Box, Transform.PointToWorld( Vector3.Left * Length / 2 ), Transform.PointToWorld( Vector3.Right * Length / 2 ) )
				.EntitiesOnly()
				.WithTag( "Goblins" )
				.Run();

			if ( trace.Hit )
			{
				Delete();
			}
		}
	}

	protected override void OnDestroy()
	{
		sceneObject.Delete();
		base.OnDestroy();
	}
}
