namespace Graphicator;

public static class Context
{
	internal static readonly List<GraphicsLayer> Layers = new();

	/// <summary> Sorts <see cref="GraphicsLayer"/>s by ZIndex </summary>
	public static void ResortLayers()
	{
		Layers.Sort( ( a, b ) => a.ZIndex - b.ZIndex );
	}

	[SceneCamera.AutomaticRenderHook]
	public class ContextDrawHook : RenderHook
	{
		public override void OnFrame( SceneCamera target )
		{
			base.OnFrame( target );

			foreach ( var layer in Layers )
			{
				layer.Update();
			}
		}

		public override void OnStage( SceneCamera target, Stage renderStage )
		{
			base.OnStage( target, renderStage );

			foreach ( var layer in Layers.Where( v => v.RenderStage == renderStage ) )
			{
				layer.Render();
			}
		}
	}
}
