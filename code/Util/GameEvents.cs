namespace GameJam;

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
}
