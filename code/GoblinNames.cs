namespace GameJam;

public static class GoblinNames
{
	private static readonly string[] PartOne = { "gl", "bl", "gr", "pr", "sk", "sn", "z", "zo" };
	private static readonly string[] PartTwo = { "imb", "umb", "erg", "arp", "org", "arz", "ax" };
	private static readonly string[] PartThree = { "y", "o", "e", "k", "it", "ax", "ik", "tz", "wort", "fist", "ly" };
	private static readonly string[] RandomSeparatorOne = { "", "", "ar", "", "", "", "ir", "", "" };
	private static readonly string[] RandomSeparatorTwo = { "-", "", "", "", "", "", "", "", "" };

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

		public Creator AddSeparatorOne()
		{
			Output += GetRandomWord( RandomSeparatorOne );
			return this;
		}

		public Creator AddSeparatorTwo()
		{
			Output += GetRandomWord( RandomSeparatorTwo );
			return this;
		}

		public Creator MakeProper()
		{
			var str = Output;
			Output = $"{str[..1].ToUpper() + str[1..]}";
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
			.AddSeparatorOne()
			.AddMiddle()
			.AddSeparatorTwo()
			.AddEnd()
			.MakeProper()
			.Output;
	}
}
