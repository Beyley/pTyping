using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.ObjectModel;
using pTyping.UiElements;
using TextDrawable = Furball.Engine.Engine.Graphics.Drawables.TextDrawable;

namespace pTyping.Graphics.Editor;

public class EditorSongFormContents : CompositeDrawable {
	private readonly Beatmap               _map;
	private readonly SliderDrawable<float> _strictnessSlider;
	private readonly DrawableTextBox       _difficultyInputAscii;
	private readonly DrawableTextBox       _difficultyInputUnicode;
	private readonly DrawableTextBox       _titleInputAscii;
	private readonly DrawableTextBox       _titleInputUnicode;
	private readonly DrawableTextBox       _artistInputAscii;
	private readonly DrawableTextBox       _artistInputUnicode;
	private readonly DrawableTextBox       _timingInputTime;
	private readonly DrawableTextBox       _timingInputTempo;

	public EditorSongFormContents(Beatmap map, EditorScreen editor) {
		this._map = map;

		const float x = 5;
		float       y = 0;

		TextDrawable strictnessLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Strictness", 24);
		y += strictnessLabel.Size.Y;

		this._strictnessSlider = new SliderDrawable<float>(new BoundNumber<float> {
			Value     = map.Difficulty.Strictness,
			MaxValue  = 10,
			MinValue  = 1,
			Precision = 0.1f
		}) {
			Position = new Vector2(x, y)
		};
		y += this._strictnessSlider.Size.Y;

		this._strictnessSlider.Value.Changed += (_, f) => {
			map.Difficulty.Strictness = f;

			editor.SaveNeeded = true;
		};

		TextDrawable difficultyNameLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Difficulty Name", 24);
		y += difficultyNameLabel.Size.Y;

		this._difficultyInputAscii   =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, this._map.Info.DifficultyName.Ascii ?? "");
		y                            += this._difficultyInputAscii.Size.Y;
		this._difficultyInputUnicode =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, this._map.Info.DifficultyName.Unicode);
		y                            += this._difficultyInputUnicode.Size.Y;

		this._difficultyInputAscii.OnCommit += (_, s) => {
			this._map.Info.DifficultyName.Ascii = s;
			editor.SaveNeeded                   = true;
		};

		this._difficultyInputUnicode.OnCommit += (_, s) => {
			this._map.Info.DifficultyName.Unicode = s;
			editor.SaveNeeded                     = true;
		};

		TextDrawable artistLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Artist", 24);
		y += artistLabel.Size.Y;

		this._artistInputAscii   =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, editor.EditorState.Set.Artist.Ascii ?? "");
		y                        += this._artistInputAscii.Size.Y;
		this._artistInputUnicode =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, editor.EditorState.Set.Artist.Unicode);
		y                        += this._artistInputUnicode.Size.Y;

		this._artistInputAscii.OnCommit += delegate(object _, string s) {
			editor.EditorState.Set.Artist.Ascii = s;
			editor.SaveNeeded                   = true;
		};

		this._artistInputUnicode.OnCommit += delegate(object _, string s) {
			editor.EditorState.Set.Artist.Unicode = s;
			editor.SaveNeeded                     = true;
		};

		TextDrawable titleLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Title (Name of song)", 24);
		y += titleLabel.Size.Y;

		this._titleInputAscii   =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, editor.EditorState.Set.Title.Ascii ?? "");
		y                       += this._titleInputAscii.Size.Y;
		this._titleInputUnicode =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, editor.EditorState.Set.Title.Unicode);
		y                       += this._titleInputUnicode.Size.Y;

		this._titleInputAscii.OnCommit += delegate(object _, string s) {
			editor.EditorState.Set.Title.Ascii = s;
			editor.SaveNeeded                  = true;
		};

		this._titleInputUnicode.OnCommit += delegate(object _, string s) {
			editor.EditorState.Set.Title.Unicode = s;
			editor.SaveNeeded                    = true;
		};

		TextDrawable timingLabel = new TextDrawable(new Vector2(x, y), pTypingGame.JapaneseFont, "Timing", 24);
		y += timingLabel.Size.Y;

		this._timingInputTime =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, $"{this._map.TimingPoints[0].Time}");
		y                     += this._timingInputTime.Size.Y;

		this._timingInputTime.OnCommit += delegate(object _, string s) {
			if (float.TryParse(s, out float time)) {
				this._map.TimingPoints[0].Time = time;
				editor.SaveNeeded              = true;
			}
		};

		this._timingInputTempo =  new DrawableTextBox(new Vector2(x, y), pTypingGame.JapaneseFont, 20, 300, $"{this._map.TimingPoints[0].Tempo}");
		y                      += this._timingInputTempo.Size.Y;

		this._timingInputTempo.OnCommit += delegate(object _, string s) {
			if (float.TryParse(s, out float tempo)) {
				this._map.TimingPoints[0].Tempo = tempo;
				editor.SaveNeeded               = true;
			}
		};

		this.Drawables.Add(strictnessLabel);
		this.Drawables.Add(this._strictnessSlider);

		this.Drawables.Add(difficultyNameLabel);
		this.Drawables.Add(this._difficultyInputAscii);
		this.Drawables.Add(this._difficultyInputUnicode);

		this.Drawables.Add(artistLabel);
		this.Drawables.Add(this._artistInputAscii);
		this.Drawables.Add(this._artistInputUnicode);

		this.Drawables.Add(titleLabel);
		this.Drawables.Add(this._titleInputAscii);
		this.Drawables.Add(this._titleInputUnicode);

		this.Drawables.Add(timingLabel);
		this.Drawables.Add(this._timingInputTime);
		this.Drawables.Add(this._timingInputTempo);
	}
}
