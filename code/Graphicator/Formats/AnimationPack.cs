namespace Graphicator.Formats;

public static class AnimationPack
{
	public struct Manifest
	{
		public struct AnimationInfo
		{
			public string Title { get; set; }
			public string Author { get; set; }
			public long Timestamp { get; set; }
		}

		public int Version { get; set; }
		public AnimationInfo Info { get; set; }
		public List<string> Entries { get; set; }

		public override string ToString() =>
			$"AnimationPack v. {Version}, {Entries?.Count ?? 0} entries, \"{Info.Title ?? "No Title"}\" by {Info.Author ?? "No Author"}";
	}
}
