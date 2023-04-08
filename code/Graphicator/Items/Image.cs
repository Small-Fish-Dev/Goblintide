using Sandbox.UI;

namespace Graphicator.Items;

public class Image : GraphicsItem
{
	public Texture Texture;

	public ImageRendering Filtering = ImageRendering.Anisotropic;

	public override void Render()
	{
		var attributes = Graphics.Attributes;
		attributes.Set( "Texture", Texture ?? Texture.Invalid );
		attributes.Set( "D_TEXTURE_FILTERING", (int)Filtering );
		Graphics.DrawQuad( ScreenspaceRect, Material.UI.Box, Color.Transparent, attributes );
	}
}
