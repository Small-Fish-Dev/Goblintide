namespace GoblinGame;

public abstract partial class GameState : BaseNetworkable
{
	public GameState()
	{
		// Register events for this GameState.
		Event.Register( this );
	}

	~GameState()
	{
		// Unregister the events.
		Event.Unregister( this );
	}
	
	/// <summary>
	/// Called when this state is created.
	/// </summary>
	public virtual void Initialize()
	{

	}

	/// <summary>
	/// Called when the state is changed to something else.
	/// </summary>
	/// <param name="state">The new state.</param>
	public virtual void Changed( GameState state )
	{
		
	}
}
