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

	public override void Tick()
	{
		base.Tick();

		foreach ( var pairing in _pairs )
		{
			/*8if ( pairing.PlaceActor == null )
			{
				// This actor doesn't exist - lets see if we should create it
				var nodeDistance = GetDistanceToCamera( pairing.Node );
				Log.Info( (nodeDistance) );
				if ( nodeDistance.x > MaxDistanceX || nodeDistance.y > MaxDistanceY )
					continue;

				// Create it!
				pairing.PlaceActor = new PlaceActor( pairing.Node ) { PanelCamera = PanelCamera };
				Content.AddChild( pairing.PlaceActor );

				continue;
			}*/

			if ( !pairing.PlaceActor.ReadyToPosition ) continue;

			var distance = GetDistanceToCamera( pairing.PlaceActor.Rect.Center );

			if ( distance.x > MaxDistanceX || distance.y > MaxDistanceY )
			{
				// Should be removed / hidden
				if ( pairing.PlaceActor != null ) pairing.PlaceActor.Style.Display = DisplayMode.None;
				continue;
			}

			// Handle fade out
			if ( distance.x > FadeDistanceX || distance.y > FadeDistanceY )
			{
				var fx = (distance.x - FadeDistanceX).Remap( 0, FadeDistanceX );
				var fy = (distance.y - FadeDistanceY).Remap( 0, FadeDistanceY );
				var f = float.Min( fx, fy );
				pairing.PlaceActor.Style.Opacity = float.Max( 0, f );
			}
			else
			{
				pairing.PlaceActor.Style.Opacity = 1;
			}

			pairing.PlaceActor.Style.Display = DisplayMode.Flex;
		}
	}
}
