namespace GameJam;

public class HostEntity<T> : Entity where T : Entity, new()
{
	private static T _instance;

	public HostEntity()
	{
		if ( _instance != null )
		{
			Delete();
			throw new Exception( $"Created {GetType().Name} while one already exists" );
		}

		Transmit = TransmitType.Always;
	}

	public static T Instance
	{
		get
		{
			if ( _instance != null ) return _instance;

			if ( Game.IsClient ) _instance = All.OfType<T>().SingleOrDefault();

			return _instance;
		}

		set
		{
			if ( Game.IsServer )
				_instance = value;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		_instance = null;
	}

	public static void Stop()
	{
		if ( Game.IsClient ) return;

		_instance?.Delete();
		_instance = null;
	}

	public static void Start()
	{
		if ( Game.IsClient ) return;

		_instance = new T();
	}
}
