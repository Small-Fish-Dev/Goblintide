﻿namespace GameJam;


public partial class WallEntity : Entity
{
	internal SceneObject sceneObject { get; set; }
	public float Length { get; set; } = 100f;
	public static BBox Box { get; set; } = new BBox( new Vector3( -10f ), new Vector3( 10f ) );

	public WallEntity( SceneWorld sceneWorld, String model, Transform transform, float length )
	{
		sceneObject = new SceneObject(sceneWorld, model, transform);
		sceneObject.Tags.Add( "nocamera" );
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
				Break();
			}
		}
	}

	public void Break()
	{
		Breakables.Break( sceneObject.Model, Position, Rotation, 1f, Color.White );
		var gibs = Entity.All
			.OfType<PropGib>()
			.Where( x => x.Position.DistanceSquared( Position ) <= Length * Length );
		foreach ( var gib in gibs )
		{
			gib.Velocity = Vector3.Random * 300;
			gib.Tags.Add( "nocamera" );
		}
		Delete();
	}

	protected override void OnDestroy()
	{
		sceneObject.Delete();
		base.OnDestroy();
	}
}