namespace GoblinGame;

public static class GameEvents
{
	/// <summary>
	/// Called on the GameManager constructor.
	/// </summary>
	public class Initialize : EventAttribute
	{
		public Initialize() : base( nameof( Initialize ) )
		{

		}
	}

	/// <summary>
	/// Called on RenderHud of GameManager.
	/// </summary>
	public class Render : EventAttribute
	{
		public Render() : base( nameof( Render ) )
		{

		}
	}
}
