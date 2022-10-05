using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Input.Events;
using pTyping.Shared.Mods;
using pTyping.Shared.Scores;

namespace pTyping.Graphics.Drawables;

public class ScoreResultsDrawable : CompositeDrawable {
	private readonly TextDrawable _username;
	private readonly TextDrawable _score;
	private readonly TextDrawable _accuracy;
	private readonly TextDrawable _combo;
	private readonly TextDrawable _excellent;
	private readonly TextDrawable _good;
	private readonly TextDrawable _fair;
	private readonly TextDrawable _poor;
	private readonly TextDrawable _mods;
	private readonly TextDrawable _date;

	private static Drawable SetRotOrigin(Drawable drawable) {
		drawable.RotationOrigin = new Vector2(-300, drawable.Size.Y / 2f);

		return drawable;
	}

	public ScoreResultsDrawable(Score score) {
		this.Drawables.Add(
			SetRotOrigin(this._username = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, $"Username: {score.User.Username}", 30))
		);
		this.Drawables.Add(SetRotOrigin(this._score = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, $"Score: {score.AchievedScore}", 30)));
		this.Drawables.Add(
			SetRotOrigin(this._accuracy = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, $"Accuracy: {score.Accuracy * 100:00.00}%", 30))
		);
		this.Drawables.Add(SetRotOrigin(this._combo = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, $"Combo: {score.MaxCombo}", 30)));
		this.Drawables.Add(
			SetRotOrigin(this._mods = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, $"Mods: {Mod.ModsShorthandString(score.Mods)}", 30))
		);
		this.Drawables.Add(
			SetRotOrigin(this._excellent = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, $"Excellent: {score.ExcellentHits}", 30))
		);
		this.Drawables.Add(SetRotOrigin(this._good = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, $"Good: {score.GoodHits}", 30)));
		this.Drawables.Add(SetRotOrigin(this._fair = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, $"Fair: {score.FairHits}", 30)));
		this.Drawables.Add(SetRotOrigin(this._poor = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, $"Poor: {score.PoorHits}", 30)));
		this.Drawables.Add(SetRotOrigin(this._date = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, $"Date: {score.Time}", 30)));

		FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;
	}

	private void OnMouseScroll(object sender, MouseScrollEventArgs mouseScrollEventArgs) {
		this._targetRotation += mouseScrollEventArgs.ScrollAmount.Y / 16f;
	}

	public override void Dispose() {
		FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;

		base.Dispose();
	}

	private const float SEPARATION_AMOUNT = 0.20f;

	private float _rotation;
	private float _targetRotation;

	public override void Update(double time) {
		float diff = this._targetRotation - this._rotation;
		this._rotation += (float)(diff * 0.98d * time);

		this._username.Rotation = this._rotation;

		this._score.Rotation    = this._rotation + SEPARATION_AMOUNT;
		this._accuracy.Rotation = this._rotation + SEPARATION_AMOUNT * 2f;
		this._combo.Rotation    = this._rotation + SEPARATION_AMOUNT * 3f;
		this._mods.Rotation     = this._rotation + SEPARATION_AMOUNT * 4f;
		this._date.Rotation     = this._rotation + SEPARATION_AMOUNT * 5f;

		this._excellent.Rotation = this._rotation - SEPARATION_AMOUNT * 4f;
		this._good.Rotation      = this._rotation - SEPARATION_AMOUNT * 3f;
		this._fair.Rotation      = this._rotation - SEPARATION_AMOUNT * 2f;
		this._poor.Rotation      = this._rotation - SEPARATION_AMOUNT;

		base.Update(time);
	}
}
