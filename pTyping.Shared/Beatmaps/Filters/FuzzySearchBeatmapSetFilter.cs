namespace pTyping.Shared.Beatmaps.Filters;

public class FuzzySearchBeatmapSetFilter : IBeatmapSetFilter {
    public string SearchQuery;

    public FuzzySearchBeatmapSetFilter(string search) {
        this.SearchQuery = search;
    }

    private const StringComparison COMPARISON = StringComparison.CurrentCultureIgnoreCase;

    public IQueryable<BeatmapSet> Filter(IQueryable<BeatmapSet> sets) {
        return sets.Where(
        set => set.Beatmaps[0].Info.Artist.Unicode.Contains(this.SearchQuery, COMPARISON) ||
               (set.Beatmaps[0].Info.Artist.Ascii != null && set.Beatmaps[0].Info.Artist.Ascii.Contains(this.SearchQuery, COMPARISON)) ||
               set.Beatmaps[0].Info.Mapper.Contains(this.SearchQuery, COMPARISON) || set.Beatmaps[0].Info.Source.Contains(this.SearchQuery, COMPARISON) ||
               (set.Beatmaps[0].Info.Title.Ascii          != null && set.Beatmaps[0].Info.Title.Ascii.Contains(this.SearchQuery, COMPARISON)) ||
               (set.Beatmaps[0].Info.DifficultyName.Ascii != null && set.Beatmaps[0].Info.DifficultyName.Ascii.Contains(this.SearchQuery, COMPARISON)) ||
               set.Beatmaps[0].Metadata.Tags.Any(x => x.Contains(this.SearchQuery, COMPARISON))
        );
    }
}
