using GameJam.UI.Core;
using Sandbox.UI;

namespace GameJam.UI;

public partial class WorldMapContent
{
	public PanelCamera PanelCamera { get; } = new();
	public Panel Content { get; set; }

	private const int MaxDistanceX = 700;
	private const int MaxDistanceY = 500;

	private const int FadeDistanceX = 400;
	private const int FadeDistanceY = 300;

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

	public Vector2 GetDistanceToCamera( Actor actor )
	{
		var a = GetActorPosition( actor.Parent, actor.Position + actor.Size / 2 );

		var b = Screen.Size;
		b *= ScaleFromScreen;
		b /= 2;

		return new Vector2(
			float.Abs( a.x - b.x ),
			float.Abs( a.y - b.y )
		);
	}

	public override void Tick()
	{
		if ( Style.Opacity == 0 ) return;

		base.Tick();

		foreach ( var pairing in _pairs )
		{
			if ( !pairing.PlaceActor.ReadyToPosition ) continue;

			var distance = GetDistanceToCamera( pairing.PlaceActor );
			
			if ( distance.x > MaxDistanceX || distance.y > MaxDistanceY )
			{
				// Should be removed / hidden
				pairing.PlaceActor.Style.Display = DisplayMode.None;
				continue;
			}

			// Handle fade out
			var fx = 1.0f;
			var fy = 1.0f;

			if ( distance.x > FadeDistanceX ) fx = distance.x.Remap( FadeDistanceX, MaxDistanceX, 1, 0 );
			if ( distance.y > FadeDistanceY ) fy = distance.y.Remap( FadeDistanceY, MaxDistanceY, 1, 0 );

			pairing.PlaceActor.Style.Opacity = float.Max( float.Min( fx, fy ), 0 );

			pairing.PlaceActor.Style.Display = DisplayMode.Flex;
		}
	}
}
