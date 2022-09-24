namespace pTyping.Shared.Beatmaps.Sorting;

public class BeatmapSetArtistComparer : IComparer<BeatmapSet> {
	public int Compare(BeatmapSet x, BeatmapSet y) {
		if (x is null || y is null) return 0;

		if (x.Beatmaps.Count == 0) return 0;
		if (y.Beatmaps.Count == 0) return 0;

		return string.Compare(
			x.Artist.ToString(),
			y.Artist.ToString(),
			StringComparison.CurrentCultureIgnoreCase
		);
	}
}
