using Sandbox.UI;

namespace GameJam;

public class SingletonPanel<T> : Panel where T : Panel, new()
{
	public static SingletonPanel<T> Instance { get; protected set; }

	public static bool IsOpen => Instance != null;

	public SingletonPanel()
	{
		Instance?.Delete( true );
		Instance = this;
	}

	public override void OnDeleted()
	{
		base.OnDeleted();

		Instance = null;
	}

	public static void Delete() => Instance?.Delete( false );

	public static void Create( Panel parent )
	{
		if ( !IsOpen )
			parent.AddChild( new T() );
	}

	public static void Toggle( Panel parent )
	{
		if ( IsOpen )
			Delete();
		else
			Create( parent );
	}

	public static void Create() => Create( HUD.Instance );
	public static void Toggle() => Toggle( HUD.Instance );
}
