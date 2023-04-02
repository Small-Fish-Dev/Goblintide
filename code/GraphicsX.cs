namespace GameJam;

/// <summary>
/// Graphics Utils - originally included in the old API
/// Made / updated by xezno (https://github.com/xezno)
///		https://gist.github.com/xezno/c890c479e32fb314f0c0cd7739afd6ea
/// Small changes by lotuspar (https://github.com/lotuspar)
/// </summary>
public static class GraphicsX
{
	private static readonly List<Vertex> VertexList = new();

	private static void AddVertex( in Vertex position ) => VertexList.Add( position );

	private static void AddVertex( in Vector2 position, in Color color, in Vector2 uv )
	{
		var vertex = new Vertex { Position = position, Color = color, TexCoord0 = uv };

		AddVertex( in vertex );
	}

	private static void MeshStart() => VertexList.Clear();

	private static void MeshEnd( RenderAttributes attr = null )
	{
		var attributes = attr ?? new RenderAttributes();

		attributes.Set( "Texture", Texture.White );

		Graphics.Draw( VertexList.ToArray(), VertexList.Count, Material.UI.Basic, attributes );
		VertexList.Clear();
	}

	public static void Line( in float startThickness, in Vector2 startPosition, in float endThickness,
		in Vector2 endPosition, in Color color, in bool handleMesh = true )
	{
		if ( handleMesh ) MeshStart();

		var directionVector = endPosition - startPosition;
		var perpendicularVector = directionVector.Perpendicular.Normal * -0.5f;

		var startCorner = startPosition + perpendicularVector * startThickness;
		var endCorner = startPosition + directionVector + perpendicularVector * endThickness;
		var endCorner2 = startPosition + directionVector - perpendicularVector * endThickness;
		var startCorner2 = startPosition - perpendicularVector * startThickness;

		var uv = new Vector2( 0f, 0f );
		AddVertex( in startCorner, in color, in uv );

		uv = new Vector2( 1f, 0f );
		AddVertex( in endCorner, in color, in uv );

		uv = new Vector2( 0f, 1f );
		AddVertex( in startCorner2, in color, in uv );

		uv = new Vector2( 1f, 0f );
		AddVertex( in endCorner, in color, in uv );

		uv = new Vector2( 1f, 1f );
		AddVertex( in endCorner2, in color, in uv );

		uv = new Vector2( 0f, 1f );
		AddVertex( in startCorner2, in color, in uv );

		if ( handleMesh ) MeshEnd();
	}

	public static void Line( in Vector2 startPosition, in Vector2 endPosition, in float thickness, in Color color ) =>
		Line( in thickness, in startPosition, in thickness, in endPosition, in color );
}
