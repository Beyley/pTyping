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

    private static ManagedDrawable SetRotOrigin(ManagedDrawable drawable) {
        drawable.RotationOrigin = new Vector2(-300, drawable.Size.Y / 2f);

        return drawable;
    }
    
    public ScoreResultsDrawable(PlayerScore playerScore) {
        this.Drawables.Add(SetRotOrigin(this._username  = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Username: {playerScore.Username}",                 30)));
        this.Drawables.Add(SetRotOrigin(this._score     = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Score: {playerScore.Score}",                       30)));
        this.Drawables.Add(SetRotOrigin(this._accuracy  = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Accuracy: {playerScore.Accuracy * 100:00.00}%",    30)));
        this.Drawables.Add(SetRotOrigin(this._combo     = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Combo: {playerScore.MaxCombo}",                    30)));
        this.Drawables.Add(SetRotOrigin(this._mods      = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Mods: {PlayerMod.GetModString(playerScore.Mods)}", 30)));
        this.Drawables.Add(SetRotOrigin(this._excellent = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Excellent: {playerScore.ExcellentHits}",           30)));
        this.Drawables.Add(SetRotOrigin(this._good      = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Good: {playerScore.GoodHits}",                30)));
        this.Drawables.Add(SetRotOrigin(this._fair = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Fair: {playerScore.FairHits}", 30)));
        this.Drawables.Add(SetRotOrigin(this._poor = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFontStroked, $"Poor: {playerScore.PoorHits}", 30)));

        FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;
    }
    
    private void OnMouseScroll(object sender, ((int scrollWheelId, float scrollAmount) scroll, string cursorName) e) {
        this._targetRotation += e.scroll.scrollAmount / 16f;
    }

    public override void Dispose() {
        FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;

        base.Dispose();
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

        this._excellent.Rotation = this._rotation - SEPARATION_AMOUNT * 4f;
        this._good.Rotation      = this._rotation - SEPARATION_AMOUNT * 3f;
        this._fair.Rotation      = this._rotation - SEPARATION_AMOUNT * 2f;
        this._poor.Rotation      = this._rotation - SEPARATION_AMOUNT;

        base.Update(time);
    }
}
