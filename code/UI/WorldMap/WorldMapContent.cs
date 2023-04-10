using GoblinGame.UI.Core;
using Sandbox.UI;

namespace GoblinGame.UI;

public partial class WorldMapContent
{
	public PanelCamera PanelCamera { get; } = new();
	public Panel Content { get; set; }

	private float _trueMaxDistanceX = 700;
	private float _trueMaxDistanceY = 700;

	private float _maxDistanceX = 500;
	private float _maxDistanceY = 500;

	private float _fadeDistanceX = 400;
	private float _fadeDistanceY = 300;

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

		// hack(gio): just move camera to where it should probably be
		PanelCamera.Position -= 400;

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

		_maxDistanceX = Content.Box.Rect.Width / 2 * ScaleFromScreen;
		_maxDistanceY = Content.Box.Rect.Height / 2 * ScaleFromScreen;

		_maxDistanceX *= 0.95f;
		_maxDistanceY *= 0.95f;

		_fadeDistanceX = _maxDistanceX * 0.89f;
		_fadeDistanceY = _maxDistanceY * 0.89f;

		_trueMaxDistanceX = _maxDistanceX * 1.2f;
		_trueMaxDistanceY = _maxDistanceY * 1.2f;

		foreach ( var pairing in _pairs )
		{
			if ( !pairing.PlaceActor.ReadyToPosition ) continue;

			var distance = GetDistanceToCamera( pairing.PlaceActor );
			if ( distance.x > _trueMaxDistanceX || distance.y > _trueMaxDistanceY )
			{
				// Should be removed / hidden
				pairing.PlaceActor.Style.Display = DisplayMode.None;
				continue;
			}

			// Handle fade out
			var fx = 1.0f;
			var fy = 1.0f;

			if ( distance.x > _fadeDistanceX ) fx = distance.x.Remap( _fadeDistanceX, _maxDistanceX, 1, 0 );
			if ( distance.y > _fadeDistanceY ) fy = distance.y.Remap( _fadeDistanceY, _maxDistanceY, 1, 0 );

			var f = float.Max( float.Min( fx, fy ), 0.01f );

			var transform = new PanelTransform();
			transform.AddScale( f );

			pairing.PlaceActor.Style.Transform = transform;

			if ( f > 0.1f )
			{
				pairing.PlaceActor.Style.Display = DisplayMode.Flex;
			}
		}
	}
}
