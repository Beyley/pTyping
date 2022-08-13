using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Vixie;
using Furball.Vixie.Backends.Shared;
using pTyping.Songs;

namespace pTyping.Graphics.Menus.SongSelect;

public class SongSelectDrawable : CompositeDrawable {
    public float TargetScroll = 0;

    private readonly List<SongButtonDrawable> _buttonDrawables = new();

    public IReadOnlyList<SongButtonDrawable> ButtonDrawables => this._buttonDrawables.AsReadOnly();

    public SongSelectDrawable(Vector2 pos, IEnumerable<Song> songList) {
        this.Position = pos;

        Texture backgroundTexture = ContentManager.LoadTextureFromFileCached("song-button-background.png", ContentSource.User);

        float y = 0;
        foreach (Song song in songList) {
            SongButtonDrawable drawable = new(new Vector2(0, y), song, backgroundTexture);

            drawable.Tags.Add(y.ToString(CultureInfo.CurrentCulture));

            this.Drawables.Add(drawable);
            this._buttonDrawables.Add(drawable);

            y += drawable.Size.Y + 7.5f;
        }
    }

    public override void Update(double time) {
        for (int i = 0; i < this._buttonDrawables.Count; i++) {
            SongButtonDrawable drawable = this._buttonDrawables[i];

            float distanceToTravel = this.TargetScroll - drawable.Position.Y + float.Parse(drawable.Tags[0]);

            drawable.Position += new Vector2(0f, (float)(distanceToTravel * time * 4));
        }
    }

    public class SongButtonDrawable : CompositeDrawable {
        public Song Song;

        private readonly TexturedDrawable _backgroundDrawable;
        private readonly TextDrawable     _titleDrawable;

        public override Vector2 Size => this._backgroundDrawable.Size * this.Scale;

        public SongButtonDrawable(Vector2 pos, Song song, Texture backgroundTexture) {
            this.Song     = song;
            this.Position = pos;

            this._backgroundDrawable = new TexturedDrawable(backgroundTexture, Vector2.Zero) {
                Scale = new Vector2(0.3f)

                // it doesnt look to good with the texture, so i think ill just leave it out for now
                // ColorOverride = song.Type == SongType.pTyping ? Color.Blue : Color.Green
            };

            this._titleDrawable = new TextDrawable(new Vector2(5), pTypingGame.JapaneseFontStroked, $"{song.Artist} - {song.Name} [{song.Difficulty}]", 30);

            this.Drawables.Add(this._titleDrawable);
            this.Drawables.Add(this._backgroundDrawable);

            this.OnClick += delegate {
                pTypingGame.CurrentSong.Value = song;
            };
        }
    }
}