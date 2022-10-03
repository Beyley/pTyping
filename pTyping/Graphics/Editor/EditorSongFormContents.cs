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

		this._strictnessSlider.Value.Changed += (sender, f) => {
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
		};

		this._difficultyInputUnicode.OnCommit += (_, s) => {
			this._map.Info.DifficultyName.Unicode = s;
		};

		this.Drawables.Add(strictnessLabel);
		this.Drawables.Add(this._strictnessSlider);

		this.Drawables.Add(difficultyNameLabel);
		this.Drawables.Add(this._difficultyInputAscii);
		this.Drawables.Add(this._difficultyInputUnicode);
	}
}
