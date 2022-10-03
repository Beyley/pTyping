using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.ObjectModel;
using pTyping.UiElements;
using TextDrawable = Furball.Engine.Engine.Graphics.Drawables.TextDrawable;

namespace pTyping.Graphics.Editor;

public class EditorSongFormContents : CompositeDrawable {
	private readonly Beatmap               _map;
	private readonly SliderDrawable<float> _strictnessSlider;

	public EditorSongFormContents(Beatmap map, EditorScreen screen) {
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

			screen.SaveNeeded = true;
		};

		this.Drawables.Add(strictnessLabel);
		this.Drawables.Add(this._strictnessSlider);
	}
}
