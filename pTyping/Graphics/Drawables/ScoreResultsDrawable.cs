using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using pTyping.Graphics.Player.Mods;
using pTyping.Scores;

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

    public ScoreResultsDrawable(PlayerScore playerScore) {
        this._username = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Username: {playerScore.Username}",                 30);
        this._score    = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Score: {playerScore.Score}",                       30);
        this._accuracy = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Accuracy: {playerScore.Accuracy * 100:00.00}%",    30);
        this._combo    = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Combo: {playerScore.MaxCombo}",                    30);
        this._mods     = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Mods: {PlayerMod.GetModString(playerScore.Mods)}", 30);

        this._excellent = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Excellent: {playerScore.ExcellentHits}", 30);
        this._good      = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Good: {playerScore.ExcellentHits}",      30);
        this._fair      = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Fair: {playerScore.ExcellentHits}",      30);
        this._poor      = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Poor: {playerScore.ExcellentHits}",      30);

        this._username.RotationOrigin = new Vector2(-300, this._username.Size.Y / 2f);
        this._score.RotationOrigin    = new Vector2(-300, this._score.Size.Y    / 2f);
        this._accuracy.RotationOrigin = new Vector2(-300, this._accuracy.Size.Y / 2f);
        this._combo.RotationOrigin    = new Vector2(-300, this._combo.Size.Y    / 2f);
        this._mods.RotationOrigin     = new Vector2(-300, this._mods.Size.Y     / 2f);

        this._excellent.RotationOrigin = new Vector2(-300, this._excellent.Size.Y / 2f);
        this._good.RotationOrigin      = new Vector2(-300, this._good.Size.Y      / 2f);
        this._fair.RotationOrigin      = new Vector2(-300, this._fair.Size.Y      / 2f);
        this._poor.RotationOrigin      = new Vector2(-300, this._poor.Size.Y      / 2f);

        this.Drawables.Add(this._username);
        this.Drawables.Add(this._score);
        this.Drawables.Add(this._accuracy);
        this.Drawables.Add(this._combo);
        this.Drawables.Add(this._mods);

        this.Drawables.Add(this._excellent);
        this.Drawables.Add(this._good);
        this.Drawables.Add(this._fair);
        this.Drawables.Add(this._poor);

        FurballGame.InputManager.OnMouseMove += this.OnMouseMove;
    }

    public override void Dispose() {
        FurballGame.InputManager.OnMouseMove -= this.OnMouseMove;

        base.Dispose();
    }

    private void OnMouseMove(object sender, (Vector2 position, string cursorName) e) {
        this._targetRotation = (e.position.Y - FurballGame.DEFAULT_WINDOW_HEIGHT / 2f) / (FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
    }

    private const float SEPARATION_AMOUNT = 0.15f;

    private float _rotation       = 0;
    private float _targetRotation = 0;

    public override void Update(double time) {
        float diff = this._targetRotation - this._rotation;
        this._rotation += (float)(diff * 0.98d * time);
        
        this._username.Rotation = this._rotation;

        this._score.Rotation    = this._rotation + SEPARATION_AMOUNT;
        this._accuracy.Rotation = this._rotation + SEPARATION_AMOUNT * 2f;
        this._combo.Rotation    = this._rotation + SEPARATION_AMOUNT * 3f;
        this._mods.Rotation     = this._rotation + SEPARATION_AMOUNT * 4f;

        this._excellent.Rotation = this._rotation - SEPARATION_AMOUNT;
        this._good.Rotation      = this._rotation - SEPARATION_AMOUNT * 2f;
        this._fair.Rotation      = this._rotation - SEPARATION_AMOUNT * 3f;
        this._poor.Rotation      = this._rotation - SEPARATION_AMOUNT * 4f;

        base.Update(time);
    }
}
