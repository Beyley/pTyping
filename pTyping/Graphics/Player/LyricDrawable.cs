using System.Collections.Generic;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Vixie.Graphics;
using pTyping.Songs;
using pTyping.Songs.Events;

namespace pTyping.Graphics.Player {
    public class LyricDrawable : CompositeDrawable {
        private readonly List<LyricEvent> _lyrics = new();

        private readonly TextDrawable _currentLyricText;
        private readonly TextDrawable _nextLyricText;

        private double _lastTime = 0;

        public LyricDrawable(Vector2 pos, Song song) {
            foreach (Event e in song.Events)
                if (e is LyricEvent le)
                    this._lyrics.Add(le);

            this.Position = pos;

            this._currentLyricText = new(new(0), pTypingGame.JapaneseFont, "", 35);
            this._drawables.Add(this._currentLyricText);

            this._nextLyricText = new(new(0), pTypingGame.JapaneseFont, "", 27) {
                ColorOverride = Color.LightGray
            };
            this._drawables.Add(this._nextLyricText);
        }

        public void UpdateLyric(double time) {
            LyricEvent thisLyric = new() {
                Lyric = string.Empty
            };
            LyricEvent nextLyric = new() {
                Lyric = string.Empty
            };

            if (this._lastTime is not 0 && this._lyrics.Count != 0) {
                if (time < this._lyrics[0].Time)
                    thisLyric = this._lyrics[0];

                for (int i = 0; i < this._lyrics.Count; i++) {
                    LyricEvent lyric = this._lyrics[i];
                    if (lyric.Time < time) {
                        thisLyric = lyric;

                        if (i != this._lyrics.Count - 1)
                            nextLyric = this._lyrics[i + 1];
                    }
                }
            }

            this._currentLyricText.Text = $"{thisLyric.Lyric}";
            this._nextLyricText.Text    = $"{nextLyric.Lyric}";

            this._currentLyricText.MoveTo(new(0, 0));
            this._nextLyricText.MoveTo(new(0, this._currentLyricText.Size.Y + 10));

            this._lastTime = time;
        }
    }
}
