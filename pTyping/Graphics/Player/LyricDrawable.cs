using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Events;

namespace pTyping.Graphics.Player;

public class LyricDrawable : CompositeDrawable {
	private readonly List<Event> _lyrics = new List<Event>();

	private readonly TextDrawable _currentLyricText;
	// private readonly TextDrawable _nextLyricText;

	public LyricDrawable(Vector2 pos, Beatmap song) {
		foreach (Event e in song.Events)
			if (e.Type == EventType.Lyric)
				this._lyrics.Add(e);

		this._lyrics.Reverse();

		this.Position = pos;

		this._currentLyricText = new TextDrawable(new Vector2(0), pTypingGame.JapaneseFont, "", 35);
		this.Drawables.Add(this._currentLyricText);

		// this._nextLyricText = new TextDrawable(new Vector2(0), pTypingGame.JapaneseFont, "", 27) {
		// ColorOverride = Color.LightGray
		// };
		// this.Drawables.Add(this._nextLyricText);
	}

	private void SetLyric(Event lyric) {
		this._currentLyricText.Text = $"{lyric.Text}";
	}

	private static readonly Event _default = new Event {
		Text  = "",
		Start = 0,
		End   = double.PositiveInfinity
	};

	public void UpdateLyric(double time) {
		this.SetLyric(this._lyrics.FirstOrDefault(lyric => lyric.Start < time && lyric.End > time, _default));
	}
}
