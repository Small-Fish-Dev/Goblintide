namespace GameJam;

public class SceneImage
{
	public Texture Color { get; private set; }
	public Action Callback { get; private set; }

	SceneCamera camera;
	static List<SceneImage> queue = new();

	public static Texture Get( SceneCamera camera, Vector2? size = null, Action? callback = null )
	{
		var sceneImage = new SceneImage()
		{
			Callback = callback,
			Color = Texture.CreateRenderTarget()
				.WithSize( size ?? 512 )
				.WithScreenFormat()
				.WithScreenMultiSample()
				.Create(),
			camera = camera
		};

		queue.Add( sceneImage );
		return sceneImage.Color;
	}

	[GameEvents.Render]
	private static void render()
	{
		for ( int i = 0; i < queue.Count; i++ )
		{
			var sceneImage = queue[i];
			if ( sceneImage == null )
				continue;

			Graphics.RenderToTexture( sceneImage.camera, sceneImage.Color );
			queue.Remove( sceneImage );
		}
	}
}
