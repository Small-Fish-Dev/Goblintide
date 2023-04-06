namespace GameJam;

public static class PrefabExtensions
{
	private static Dictionary<Prefab, Texture> cache = new();

	/// <summary>
	/// Creates a Texture icon for a prefab.
	/// </summary>
	/// <param name="prefab"></param>
	/// <returns></returns>
	public static Texture GetIcon( this Prefab prefab )
	{
		if ( cache.TryGetValue( prefab, out var tex ) )
			return tex;

		// Initial values.
		var root = prefab.Root;
		var model = Model.Load( root.GetValue<string>( "Model" ) );
		if ( model == null )
			return null;

		// Position and rotation according to model.
		var pos = model.Bounds.Center
			+ Vector3.Up * 20f
			+ Vector3.Backward * (25f + model.Bounds.Maxs.x * 2);
		var rot = Rotation.LookAt( model.Bounds.Center - pos );

		// Create scene.
		var world = new SceneWorld();
		var obj = new SceneObject( world, model, new Transform( Vector3.Zero, Rotation.FromYaw( 45f ) ) );
		var light = new SceneLight( world )
		{
			Position = pos,
			Rotation = rot.Inverse,
			LightColor = Color.White * 4f,
			Radius = 1000,
		};

		var camera = new SceneCamera( "sceneimage" )
		{
			World = world,
			Position = pos,
			Rotation = rot
		};

		// Capture texture.
		var sceneImage = SceneImage.Get( camera, 256 );
		world = null;
		camera = null;
		
		// Cache value.
		cache.Add( prefab, sceneImage );

		return sceneImage;
	}
}
