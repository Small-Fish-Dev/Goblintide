using Sandbox.UI;

namespace GameJam.UI;

public class AnimatedImage : Image
{
	private readonly List<Texture> _textures = new();
	private readonly List<Texture> _transition = new();

	private float _changeDelay = 1;
	private float _transitionDelay = 1;

	private int _index;

	private bool _transitioning;
	private bool _invert;

	// hack
	private bool _loaded;

	private RealTimeUntil _next = 0.5f;

	public override void Tick()
	{
		base.Tick();

		if ( _next )
			Next();
	}

	private void Next()
	{
		_next = _transitioning ? _transitionDelay : _changeDelay;

		var input = _transitioning ? _transition : _textures;

		_index++;

		if ( _index >= input.Count )
		{
			_index = 0;
			if ( _transitioning ) _transitioning = false;
		}

		input = _transitioning ? _transition : _textures;

		if ( input.Count != 0 )
			Texture = input[!_invert ? _index : input.Count - _index];
	}

	private void LoadSrc( string path, bool isTransition )
	{
		var output = isTransition ? _transition : _textures;

		void LoadTexture( string src )
		{
			var texture = Texture.Load( FileSystem.Mounted, src );
			if ( texture == null )
			{
				Log.Warning( $"Couldn't load texture @ {src}" );
				return;
			}

			output.Add( texture );
		}

		if ( isTransition ) _transitioning = true;

		var fs = FileSystem.Mounted;
		if ( fs.DirectoryExists( path ) )
		{
			// src is a directory
			foreach ( var file in fs.FindFile( path, "*.png" ) )
				LoadTexture( $"{path}/{file}" );
		}
		else if ( fs.FileExists( path ) )
		{
			// src is a file
			LoadTexture( path );
		}
		else
		{
			// src not found
			Log.Warning( $"Texture file / folder @ {path} not found" );
		}
	}

	protected override void OnParametersSet()
	{
		base.OnParametersSet();
		_loaded = true;

		if ( _transition.Count != 0 )
		{
			_transitioning = true;
			Texture = _transition[0];
		}
		else if ( _textures.Count != 0 ) Texture = _textures[0];

		_next = _transitioning ? _transitionDelay : _changeDelay;
	}

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( _loaded ) return;
		switch ( name )
		{
			case "delay":
				_changeDelay = value.ToFloat( 1 );
				break;
			case "transition-delay":
				_transitionDelay = value.ToFloat( 1 );
				break;
			case "src":
				LoadSrc( value, false );
				break;
			case "transition-src":
				LoadSrc( value, true );
				break;
		}
	}
}
