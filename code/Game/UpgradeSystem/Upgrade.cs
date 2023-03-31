namespace GameJam.UpgradeSystem;

[AttributeUsage( AttributeTargets.Property )]
public class EffectAttribute : Attribute
{
}

public class Upgrade
{
	private static readonly List<Upgrade> BuiltUpgrades = new();
	public static IEnumerable<Upgrade> All => BuiltUpgrades.AsReadOnly();

	public Upgrade() { }

	private Upgrade( string identifier, string title )
	{
		Identifier = identifier;
		Title = title;
	}

	public string Identifier { get; }
	public string Title { get; }
	public int Cost = 10;

	public List<string> Dependencies;

	#region Effects

	[Effect] public float ExperienceGain { get; set; }
	[Effect] public float MovementSpeed { get; set; }
	[Effect] public float DecreasedAreaDiligence { get; set; }

	#endregion

	public override string ToString() => $"{Identifier} ({Title}): {Dependencies?.Count} deps, {Cost} cost";

	public struct Builder
	{
		private List<Builder> _next;
		private readonly string _identifier;
		private readonly string _title;

		private Action<Upgrade> _postBuild;

		private readonly string _dependency;
		private string _last;

		public Builder( string identifier )
		{
			_identifier = identifier;
			_title = identifier;
			_last = identifier;
		}

		private Builder( string identifier, string dependency ) : this( identifier )
		{
			_dependency = dependency;
		}

		public Builder ConfigureWith( Action<Upgrade> action )
		{
			_postBuild = action;
			return this;
		}

		public Builder Next( string identifier, Func<Builder, Builder> creator = null )
		{
			_next ??= new List<Builder>();
			var builder = new Builder( identifier, _last ) { _postBuild = _postBuild };
			var next = creator?.Invoke( builder ) ?? builder;
			_last = next._identifier;
			_next.Add( next );
			return this;
		}

		public Upgrade Build()
		{
			var instance = new Upgrade( _identifier, _title );
			_postBuild?.Invoke( instance );

			instance.Dependencies ??= new List<string>();

			if ( _dependency != null )
				instance.Dependencies.Add( _dependency );

			BuiltUpgrades.Add( instance );

			if ( _next == null )
				return instance;

			foreach ( var builder in _next )
				builder.Build();

			return instance;
		}
	}
}
