namespace GameJam;

public static class GoblinNames
{
	private static readonly string[] PartOne = { "gl", "bl", "gr" };
	private static readonly string[] PartTwo = { "imb", "umb", "erg" };
	private static readonly string[] PartThree = { "y", "o", "e" };

	public struct Creator
	{
		public string Output;

		private static string GetRandomWord( IReadOnlyList<string> array ) => array[Game.Random.Next( array.Count )];

		public Creator AddStart()
		{
			Output += GetRandomWord( PartOne );
			return this;
		}

		public Creator AddMiddle()
		{
			Output += GetRandomWord( PartTwo );
			return this;
		}

		public Creator AddEnd()
		{
			Output += GetRandomWord( PartThree );
			return this;
		}

		public Creator MakeProper()
		{
			Output = Output.ToTitleCase();
			return this;
		}
	}

	/// <summary>
	/// Create a funny goblin name
	/// </summary>
	/// <returns>Funny goblin name</returns>
	public static string Create()
	{
		return new Creator()
			.AddStart()
			.AddMiddle()
			.AddEnd()
			.MakeProper()
			.Output;
	}
}
