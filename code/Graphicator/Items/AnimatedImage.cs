namespace Graphicator.Items;

public class AnimatedImage : Image
{
	/// <summary> Use <see cref="Add"/> to add / set textures </summary>
	public new Texture Texture => _currentTexture;

	internal enum PartType
	{
		Texture,
		SetDelay
	}

	internal struct Part
	{
		public PartType Type;
		public object Data;

		public Part( Texture texture )
		{
			Type = PartType.Texture;
			Data = texture;
		}
	}

	private class BuiltAnimation
	{
		public string Name;
		public List<Part> Parts;
		public int Progress;
		public bool Repeating;

		public bool IsComplete => Progress >= Parts.Count;

		public void Reset() => Progress = 0;
	}

	public struct Animation
	{
		public readonly string Name;

		public bool Repeating;

		internal List<Part> Parts;

		public Animation() : this( "main" ) { }
		public Animation( string name ) => Name = name;

		public Animation Texture( Texture texture )
		{
			Parts ??= new List<Part>();
			Parts.Add( new Part( texture ) );
			return this;
		}

		public Animation Delay( float value )
		{
			Parts ??= new List<Part>();
			Parts.Add( new Part { Type = PartType.SetDelay, Data = value } );
			return this;
		}

		public Animation ShouldRepeat( bool value )
		{
			Repeating = value;
			return this;
		}

		public static Animation FromFolder( string name, string src )
		{
			Animation animation = new(name);

			if ( !FileSystem.Mounted.DirectoryExists( src ) )
				throw new Exception( $"Folder {src} not found" );

			var files = FileSystem.Mounted.FindFile( src, "*.png" ).Order();

			foreach ( var file in files )
			{
				var path = FileSystem.NormalizeFilename( $"{src}/{file}" );
				var texture = Sandbox.Texture.Load( path );
				if ( texture == null )
					throw new Exception( $"Failed to open {file} as a texture" );
				animation = animation.Texture( texture );
			}

			return animation;
		}
	}

	private readonly List<BuiltAnimation> _animations = new();
	private readonly Stack<BuiltAnimation> _stack = new();
	private TimeUntil _next = 0.0f;
	private float _delay = 0.1f;
	private Texture _currentTexture;

	/// <summary> Add animation to animation storage </summary>
	/// <param name="animation">Animation</param>
	public void Add( Animation animation )
	{
		if ( _animations.Any( v => v.Name == animation.Name ) )
			throw new Exception( $"Animation with name {animation.Name} already exists" );

		if ( animation.Parts == null )
			throw new Exception( "Added exception has no parts" );

		var built = new BuiltAnimation
		{
			Name = animation.Name, Parts = animation.Parts, Repeating = animation.Repeating
		};

		_animations.Add( built );

		if ( built.Name == "main" )
		{
			Log.Info( "Autoplaying animation called main" );
			Push( built );
		}
	}

	private void Push( BuiltAnimation built )
	{
		if ( _stack.TryPeek( out var top ) )
		{
			if ( top == built )
				Log.Warning( $"{built} pushed but it was already top of the stack!" );
		}

		_stack.Push( built );
	}

	public void Push( string name )
	{
		var built = _animations.FirstOrDefault( v => v.Name == name );
		if ( built == null )
			throw new Exception( $"Animation with name {name} not found" );
		Push( built );
	}

	public override void Render()
	{
		RenderImage( Texture );
	}

	private void ProcessBuiltAnimationUpdate( BuiltAnimation built )
	{
		while ( !built.IsComplete )
		{
			var part = built.Parts[built.Progress];
			built.Progress++;
			switch ( part.Type )
			{
				case PartType.Texture:
					_currentTexture = (Texture)part.Data;
					return;
				case PartType.SetDelay:
					_delay = (float)part.Data;
					break;
				default:
					throw new ArgumentOutOfRangeException( $"Unknown part type {part.Type}" );
			}
		}
	}

	public override void Update()
	{
		base.Update();

		if ( !_next ) return;

		if ( !_stack.TryPeek( out var built ) ) return;

		// Handle animation
		ProcessBuiltAnimationUpdate( built );

		// Pop stack if animation completed (and we need to pop stack)
		if ( built.IsComplete )
		{
			built.Reset();

			if ( !built.Repeating ) _stack.Pop();
		}

		// Set timer
		_next = _delay;
	}
}
