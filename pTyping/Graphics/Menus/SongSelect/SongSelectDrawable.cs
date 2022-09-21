using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Vixie;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.Filters;
using pTyping.Shared.Beatmaps.Sorting;
using Realms;

namespace pTyping.Graphics.Menus.SongSelect;

public class SongSelectDrawable : CompositeDrawable {
    private readonly List<BeatmapSetDrawable> _registeredSetButtons = new();
    
    public readonly  ObservableCollection<IBeatmapSetFilter> FilterOperations = new();

    public SongSelectDrawable(Vector2 pos) {
        this.Position = pos;
        
        this.FilterOperations.CollectionChanged += this.OnFilterOperationChange;
        
        this.UpdateDrawables();
    }
    
    private void OnFilterOperationChange(object sender, NotifyCollectionChangedEventArgs e) {
        this.UpdateDrawables();
    }

    private void UpdateDrawables() {
        IQueryable<BeatmapSet> sets = pTypingGame.BeatmapDatabase.Realm.All<BeatmapSet>();

        foreach (IBeatmapSetFilter filter in this.FilterOperations) {
            sets = filter.Filter(sets);
        }

        ImmutableSortedSet<BeatmapSet> sortedSets = sets.ToList().Where(x => x.Beatmaps.Count != 0).ToImmutableSortedSet(new BeatmapSetArtistComparer());

        this.Drawables.Clear();
        this._registeredSetButtons.Clear();

        foreach (BeatmapSet set in sortedSets) {
            BeatmapSetDrawable drawable = new(set);
            
            this._registeredSetButtons.Add(drawable);
        }

        float y = 0;
        foreach (BeatmapSetDrawable button in this._registeredSetButtons) {
            button.Position   = new Vector2(0, y);
            // button.OriginType = OriginType.TopRight;

            button.YPositionInDrawable = y;
            
            this.Drawables.Add(button);

            y += button.Size.Y + 5;
        }
    }

    public class BeatmapButtonDrawable : CompositeDrawable {
        public Beatmap Song;

        private readonly TexturedDrawable _backgroundDrawable;
        private readonly TextDrawable     _titleDrawable;

        public override Vector2 Size => this._backgroundDrawable.Size * this.Scale;

        public BeatmapButtonDrawable(Vector2 pos, Beatmap song, Texture backgroundTexture) {
            this.Song     = song;
            this.Position = pos;

            this._backgroundDrawable = new TexturedDrawable(backgroundTexture, Vector2.Zero) {
                Scale = new Vector2(0.3f)

                // it doesnt look to good with the texture, so i think ill just leave it out for now
                // ColorOverride = song.Type == SongType.pTyping ? Color.Blue : Color.Green
            };

            this._titleDrawable = new TextDrawable(
            new Vector2(this._backgroundDrawable.Size.X - 5, 5),
            pTypingGame.JapaneseFontStroked,
            $"{song.Info.Artist} - {song.Info.Title} [{song.Info.DifficultyName}]",
            30
            ) {OriginType = OriginType.TopRight};

            this.Drawables.Add(this._titleDrawable);
            this.Drawables.Add(this._backgroundDrawable);

            this.OnClick += delegate {
                pTypingGame.CurrentSong.Value = song;
            };
        }
    }
}