using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using pTyping.Songs;
using pTyping.Songs.Events;

namespace pTyping.Graphics.Player;

public class LyricDrawable : CompositeDrawable {
    private readonly List<LyricEvent> _lyrics = new();

    private readonly TextDrawable _currentLyricText;
    // private readonly TextDrawable _nextLyricText;

    public LyricDrawable(Vector2 pos, Song song) {
        foreach (Event e in song.Events)
            if (e is LyricEvent le)
                this._lyrics.Add(le);

        this._lyrics.Reverse();

        this.Position = pos;

        this._currentLyricText = new TextDrawable(new Vector2(0), pTypingGame.JapaneseFont, "", 35);
        this.Drawables.Add(this._currentLyricText);

        // this._nextLyricText = new TextDrawable(new Vector2(0), pTypingGame.JapaneseFont, "", 27) {
        // ColorOverride = Color.LightGray
        // };
        // this.Drawables.Add(this._nextLyricText);
    }

    private void SetLyric(LyricEvent lyric) {
        this._currentLyricText.Text = $"{lyric.Lyric}";
    }

    private static readonly LyricEvent _default = new() {
        Lyric   = "",
        Time    = 0,
        EndTime = double.PositiveInfinity
    };
    
    public void UpdateLyric(double time) {
        this.SetLyric(this._lyrics.FirstOrDefault(lyric => lyric.Time < time && lyric.EndTime > time, _default));
    }
}