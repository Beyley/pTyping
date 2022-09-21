namespace pTyping.Shared.Beatmaps.Sorting; 

public class BeatmapDifficultyNameComparer : IComparer<Beatmap> {
    public int Compare(Beatmap x, Beatmap y) {
        if (x is null || y is null) throw new InvalidOperationException();
        
        return string.Compare(
        x.Info.Artist.ToString(), 
        y.Info.Artist.ToString(), 
        StringComparison.CurrentCultureIgnoreCase
        );
    }
}
