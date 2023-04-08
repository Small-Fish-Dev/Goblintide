using Sandbox.UI;

namespace Graphicator.Items;

public class Image : GraphicsItem
{
	public Texture Texture;

	public ImageRendering Filtering = ImageRendering.Anisotropic;
	public BackgroundRepeat Repeating = BackgroundRepeat.NoRepeat;

	protected void RenderImage( Texture texture )
	{
		var attributes = Graphics.Attributes;

		var rect = ScreenspaceRect;

		attributes.Set( "Texture", texture ?? Texture.Invalid );
		attributes.Set( "HasBorder", 0 );

		attributes.Set( "BgRepeat", (int)Repeating );
		attributes.Set( "BgPos", new Vector4( 0, 0, rect.Width, rect.Height ) );

		attributes.Set( "D_TEXTURE_FILTERING", (int)Filtering );
		attributes.Set( "D_BACKGROUND_IMAGE", 1 );

		attributes.Set( "BoxPosition", rect.TopLeft );
		attributes.Set( "BoxSize", rect.Size );

		Graphics.DrawQuad( ScreenspaceRect, Material.UI.Box, Color.Blue, attributes );
	}

	public override void Render() => RenderImage( Texture );
}
