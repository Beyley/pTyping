using System.Collections.Generic;
using System.Globalization;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pTyping.Songs;

namespace pTyping.Drawables {
    public class SongSelectDrawable : CompositeDrawable {
        public float TargetScroll = 0;

        private readonly List<SongButtonDrawable> _buttonDrawables = new();

        public IReadOnlyList<SongButtonDrawable> ButtonDrawables => this._buttonDrawables.AsReadOnly();

        public SongSelectDrawable(Vector2 pos, IEnumerable<Song> songList) {
            this.Position = pos;

            Texture2D backgroundTexture = ContentManager.LoadTextureFromFile("song-button-background.png", ContentSource.User);

            float y = 0;
            foreach (Song song in songList) {
                SongButtonDrawable drawable = new(new(0, y), song, backgroundTexture);

                drawable.Tags.Add(y.ToString(CultureInfo.CurrentCulture));

                this.Drawables.Add(drawable);
                this._buttonDrawables.Add(drawable);

                y += drawable.Size.Y + 7.5f;
            }
        }

        public override void Update(GameTime time) {
            for (int i = 0; i < this._buttonDrawables.Count; i++) {
                SongButtonDrawable drawable = this._buttonDrawables[i];

                float distanceToTravel = this.TargetScroll - drawable.Position.Y + float.Parse(drawable.Tags[0]);

                drawable.Position += new Vector2(0f, distanceToTravel / 200 * (time.ElapsedGameTime.Ticks / 10000f));
            }
        }

        public class SongButtonDrawable : CompositeDrawable {
            public Song Song;

            private readonly TexturedDrawable _backgroundDrawable;
            private readonly TextDrawable     _titleDrawable;

            public override Vector2 Size => this._backgroundDrawable.Size;

            public SongButtonDrawable(Vector2 pos, Song song, Texture2D backgroundTexture) {
                this.Song     = song;
                this.Position = pos;

                this._backgroundDrawable = new(backgroundTexture, Vector2.Zero) {
                    Scale = new(0.3f)

                    // it doesnt look to good with the texture, so i think ill just leave it out for now
                    // ColorOverride = song.Type == SongType.pTyping ? Color.Blue : Color.Green
                };

                this._titleDrawable = new(new(5), pTypingGame.JapaneseFontStroked, $"{song.Artist} - {song.Name} [{song.Difficulty}]", 30);

                this.Drawables.Add(this._titleDrawable);
                this.Drawables.Add(this._backgroundDrawable);

                this.OnClick += delegate {
                    pTypingGame.CurrentSong.Value = song;
                };
            }
        }
    }
}