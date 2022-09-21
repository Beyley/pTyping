namespace pTyping.Shared.Beatmaps.Filters;

public class FuzzySearchBeatmapSetFilter : IBeatmapSetFilter {
    public string SearchQuery;

    public FuzzySearchBeatmapSetFilter(string search) {
        this.SearchQuery = search;
    }

    private const StringComparison COMPARISON = StringComparison.CurrentCultureIgnoreCase;

    public IQueryable<BeatmapSet> Filter(IQueryable<BeatmapSet> sets) {
        return sets.Where(
        set => set.Artist.Unicode.Contains(this.SearchQuery, COMPARISON) || set.Artist.Ascii != null && set.Artist.Ascii.Contains(this.SearchQuery, COMPARISON) ||
               set.Beatmaps[0].Info.Mapper.Contains(this.SearchQuery, COMPARISON) || set.Source.Contains(this.SearchQuery, COMPARISON) ||
               set.Title.Ascii != null && set.Title.Ascii.Contains(this.SearchQuery, COMPARISON) ||
               (set.Beatmaps[0].Info.DifficultyName.Ascii != null && set.Beatmaps[0].Info.DifficultyName.Ascii.Contains(this.SearchQuery, COMPARISON)) ||
               set.Beatmaps[0].Metadata.Tags.Any(x => x.Contains(this.SearchQuery, COMPARISON))
        );
    }
}
