namespace pTyping.Shared.Beatmaps.Sorting; 

public class BeatmapSetArtistComparer : IComparer<BeatmapSet> {
    public int Compare(BeatmapSet x, BeatmapSet y) {
        if (x is null || y is null) throw new InvalidOperationException();
        
        if (x.Beatmaps.Count == 0) throw new InvalidOperationException();
        if (y.Beatmaps.Count == 0) throw new InvalidOperationException();

        return string.Compare(
        x.Beatmaps.First().Info.Artist.ToString(), 
        y.Beatmaps.First().Info.Artist.ToString(), 
        StringComparison.CurrentCultureIgnoreCase
        );
    }
}
