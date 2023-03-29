namespace GameJam;

public static class Names
{
	public enum Type
	{
		Human,
		Goblin
	}

	private static string getRandom( IReadOnlyList<string> array ) 
		=> array[Game.Random.Next( array.Count )];

	private static readonly string[] goblinPartOne = { "gl", "bl", "gr", "pr", "sk", "sn", "z", "zo", "sh" };
	private static readonly string[] goblinPartTwo = { "imb", "umb", "erg", "arp", "org", "arz", "ax", "mor" };
	private static readonly string[] goblinPartThree = { "y", "o", "e", "k", "it", "ax", "ik", "tz", "wort", "fist", "ly", "by" };
	private static readonly string[] goblinRandomSeparatorOne = { "", "", "ar", "", "", "", "ir", "", "" };
	private static readonly string[] goblinRandomSeparatorTwo = { "-", "", "", "", "", "", "", "", "" };

	private static readonly string[] humanPartOne = { "Henry", "Edward", "John", "Stephen", "Geoffrey", "Thomas", "Walter", "Roger", "Alan", "Gilbert", "Bartholomew", "Hugh", "Philip", "Baldwin", "Godfrey", "Reginald", "Simon", "Thorfinn"  };
	private static readonly string[] humanPartTwo = { "Fitzroy", "Beaumont", "Lancaster", "Plantagenet", "Fitzwilliam", "Devereux", "Talbot", "Neville", "Howard", "Percy", "Montagu", "De Clare", "Mortimer", "Stafford", "Wycliffe", "Berkeley", "Ferrers", "Lovell", "Grey", "Hastings" };

	public static string Create( Type type = Type.Human )
	{
		switch ( type )
		{
			case Type.Human:
				return $"{getRandom( humanPartOne )} {getRandom( humanPartTwo )}";

			case Type.Goblin:
			{
				var result = $"{getRandom( goblinPartOne )}" +
					$"{getRandom( goblinRandomSeparatorOne )}" +
					$"{getRandom( goblinPartTwo )}" +
					$"{getRandom( goblinRandomSeparatorTwo )}" +
					$"{getRandom( goblinPartThree )}";

				return $"{result[..1].ToUpper() + result[1..]}";
			}

			default:
				return "";
		}
	}
}
