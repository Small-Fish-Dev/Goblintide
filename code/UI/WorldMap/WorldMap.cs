using System.Threading;
using Sandbox.UI;

namespace GameJam.UI;

public partial class WorldMap
{
	public Panel Container { get; set; }
	public Panel Content { get; set; }

	private const int MaxDistanceX = 700;
	private const int MaxDistanceY = 500;

	private const int FadeDistanceX = 550;
	private const int FadeDistanceY = 400;

	public class PlacePairing
	{
		public readonly WorldMapHost.Node Node;
		public PlaceActor PlaceActor;
		public bool Appended;

		public PlacePairing( WorldMapHost.Node a, PlaceActor b )
		{
			Node = a;
			PlaceActor = b;
		}
	}

	public readonly List<PlacePairing> Pairings = new();

	private async void Initialize()
	{
		Pairings.Clear();

		await GameTask.RunInThreadAsync( () =>
		{
			RealTimeSince timer = 0;
			Log.Info( "Generating initial pairings..." );

			foreach ( var entry in WorldMapHost.Entries )
			{
				Pairings.Add( new PlacePairing( entry, null ) );
			}

			Log.Info( $"Generated {Pairings.Count} pairings in {timer.Relative * 1000}ms." );

			timer = 0;
			Log.Info( "Generating actors..." );
			foreach ( var pairing in Pairings )
			{
				pairing.PlaceActor = new PlaceActor( pairing.Node );
				pairing.PlaceActor.PanelCamera = Camera;
			}

			Log.Info( $"Generated in {timer.Relative * 1000}ms." );

			timer = 0;
			Log.Info( "Appending actors..." );

			lock ( Pairings )
			{
				foreach ( var pairing in Pairings )
				{
					var distance = GetDistanceToCamera( pairing.Node );
					if ( distance.x > MaxDistanceX || distance.y > MaxDistanceY )
						continue;
					Content.AddChild( pairing.PlaceActor );
				}
			}

			Log.Info( $"Appended in {timer.Relative * 1000}ms." );

			_appendingDone = true;
		} );
	}
	
	private bool _appendingDone;

	public WorldMap()
	{
		SkillTree.Delete();
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( firstTime )
			Initialize();
	}

	public override void OnDeleted()
	{
		base.OnDeleted();

		foreach ( var pairing in Pairings )
		{
			pairing.PlaceActor?.Delete( immediate: true );
			pairing.PlaceActor = null;
		}

		Pairings.Clear();
	}

	public Vector2 GetActorPosition( Panel parent, Vector2 position )
	{
		// Actor pos
		var a = position;

		// Remove camera offset from actor pos
		a -= Camera.Position;

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
		a -= Camera.Position;

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

		// Handle actors based on distance
		if ( !_appendingDone )
			return;

		foreach ( var pairing in Pairings )
		{
			if ( pairing.PlaceActor == null )
			{
				// This actor doesn't exist - lets see if we should create it
				var nodeDistance = GetDistanceToCamera( pairing.Node );
				if ( nodeDistance.x > MaxDistanceX || nodeDistance.y > MaxDistanceY )
					continue;

				// Create it!
				pairing.PlaceActor = new PlaceActor( pairing.Node );
				pairing.PlaceActor.PanelCamera = Camera;
				Content.AddChild( pairing.PlaceActor );
				
				continue;
			}

			if ( !pairing.Appended ) continue;
			
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
