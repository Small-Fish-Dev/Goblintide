using GameJam.UI.Core;
using Sandbox.UI;

namespace GameJam.UI;

public partial class WorldMapContent
{
	public PanelCamera PanelCamera { get; } = new();
	public Panel Content { get; set; }

	private const int MaxDistanceX = 700;
	private const int MaxDistanceY = 500;

	private const int FadeDistanceX = 550;
	private const int FadeDistanceY = 400;
	
	private class Pairing
	{
		public readonly WorldMapHost.Node Node;
		public PlaceActor PlaceActor;

		public Pairing( WorldMapHost.Node a, PlaceActor b )
		{
			Node = a;
			PlaceActor = b;
		}
	}

	private readonly List<Pairing> _pairs = new();

	public WorldMapContent()
	{
		if ( _instance != null )
			throw new Exception( "Created secondary WorldMapContent" );
		_instance = this;

		Hide();
		
		// Create pairs
		if ( WorldMapHost.Entries == null || WorldMapHost.Entries.Count == 0 )
			throw new Exception( "WorldMapHost not ready but WorldMapContent created" );

		foreach ( var entry in WorldMapHost.Entries )
			_pairs.Add( new Pairing( entry, null ) );
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime ) return;
		
		// Create actors for entries that should be instantly visible
		foreach ( var pairing in _pairs )
		{
			pairing.PlaceActor = new PlaceActor( pairing.Node );
			pairing.PlaceActor.PanelCamera = PanelCamera;
			var distance = GetDistanceToCamera( pairing.Node );
			if ( distance.x > MaxDistanceX || distance.y > MaxDistanceY )
				continue;
			Content.AddChild( pairing.PlaceActor );
		}
	}

	public Vector2 GetActorPosition( Panel parent, Vector2 position )
	{
		// Actor pos
		var a = position;

		// Remove camera offset from actor pos
		a -= PanelCamera.Position;

		// Remove parent offset from actor pos
		a += (parent?.Box.Rect.Position ?? Vector2.Zero) * ScaleFromScreen;

		return a;
	}

	public Vector2 GetDistanceToCamera( Vector2 position )
	{
		var a = GetActorPosition( Content, position );

		var b = Screen.Size;
		b *= ScaleFromScreen;
		b /= 2;

		return new Vector2(
			float.Abs( a.x - b.x ),
			float.Abs( a.y - b.y )
		);
	}
	
	public Vector2 GetDistanceToCamera( WorldMapHost.Node node )
	{
		var a = node.MapPosition;
		a -= PanelCamera.Position;

		var b = Screen.Size;
		b *= ScaleFromScreen;
		b /= 2;

		var c = Content.Box.Rect.Size;
		c /= 2;
		c *= ScaleFromScreen;

		a += c;
		
		return new Vector2(
			float.Abs( a.x - b.x ),
			float.Abs( a.y - b.y )
		);
	}

}
