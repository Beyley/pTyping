namespace pTyping.Shared.Beatmaps.Filters; 

public interface IBeatmapSetFilter {
    public IQueryable<BeatmapSet> Filter(IQueryable<BeatmapSet> sets);
}
