using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Vixie.Backends.Shared;
using pTyping.Songs;
using pTyping.Songs.Events;

namespace pTyping.Graphics.Player;

public class LyricDrawable : CompositeDrawable {
    private readonly List<LyricEvent> _lyrics = new();

    private readonly TextDrawable _currentLyricText;
    private readonly TextDrawable _nextLyricText;

    public LyricDrawable(Vector2 pos, Song song) {
        foreach (Event e in song.Events)
            if (e is LyricEvent le)
                this._lyrics.Add(le);

        this._lyrics.Reverse();

        this.Position = pos;

        this._currentLyricText = new(new(0), pTypingGame.JapaneseFont, "", 35);
        this.Drawables.Add(this._currentLyricText);

        this._nextLyricText = new(new(0), pTypingGame.JapaneseFont, "", 27) {
            ColorOverride = Color.LightGray
        };
        this.Drawables.Add(this._nextLyricText);
    }

    public void UpdateLyric(double time) {
        LyricEvent thisLyric = new() {
            Lyric = string.Empty
        };
        LyricEvent nextLyric = new() {
            Lyric = string.Empty
        };

        if (this._lyrics.Count == 0) goto end;

        if (time < this._lyrics[^1].Time) {
            nextLyric = this._lyrics[^1];
            goto end;
        }

        thisLyric = this._lyrics.First(x => x.Time < time);
        int thisIndex = this._lyrics.IndexOf(thisLyric);

        if (thisIndex != 0)
            nextLyric = this._lyrics[thisIndex - 1];

    end:

        this._currentLyricText.Text = $"{thisLyric.Lyric}";
        this._nextLyricText.Text    = $"{nextLyric.Lyric}";

        this._nextLyricText.MoveTo(new(0, this._currentLyricText.Font.LineHeight * this._currentLyricText.RealScale.Y + 10));

        // this._lastUpdate = time;
    }
}