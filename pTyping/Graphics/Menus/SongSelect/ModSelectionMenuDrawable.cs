using System;
using System.Collections.Generic;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Player.Mods;

namespace pTyping.Graphics.Menus.SongSelect;

public class ModSelectionMenuDrawable : CompositeDrawable {
    private readonly List<ModButtonDrawable> _mods = new();

    private readonly TextDrawable _scoreMultiplier;

    public event EventHandler OnModAdd;

    public void OnModClick(object sender, bool added) {
        ModButtonDrawable modButton = (ModButtonDrawable)sender;

        foreach (ModButtonDrawable modButtonDrawable in this._mods)
            if (modButton != modButtonDrawable)
                modButtonDrawable.ModStateChange(modButton, added);

        this.UpdateScoreMultiplierText();

        this.OnModAdd?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateScoreMultiplierText() {
        this._scoreMultiplier.Text = $"Score Multiplier: {PlayerMod.ScoreMultiplier(pTypingGame.SelectedMods):#0.##}x";
    }

    public bool Shown = true;

    private readonly List<LinePrimitiveDrawable> Lines = new();
    private readonly RectanglePrimitiveDrawable  _background;

    public void Hide(bool force = false) {
        foreach (ModButtonDrawable modButtonDrawable in this._mods)
            modButtonDrawable.Hide(force);
        this.Shown = false;

        float time = force ? 0 : 500;

        foreach (LinePrimitiveDrawable line in this.Lines)
            line.Tweens.Add(new FloatTween(TweenType.Fade, line.ColorOverride.Af, 0f, FurballGame.Time, FurballGame.Time + time));

        this._scoreMultiplier.Tweens.Add(new FloatTween(TweenType.Fade, this._scoreMultiplier.ColorOverride.Af, 0f, FurballGame.Time, FurballGame.Time + time));
        this._background.Tweens.Add(new FloatTween(TweenType.Fade,      this._background.ColorOverride.Af,      0f, FurballGame.Time, FurballGame.Time + time));
    }

    public void Show() {
        foreach (ModButtonDrawable modButtonDrawable in this._mods)
            modButtonDrawable.Show();
        this.Shown = true;

        const float time = 500;

        foreach (LinePrimitiveDrawable line in this.Lines)
            line.Tweens.Add(new FloatTween(TweenType.Fade, line.ColorOverride.Af, 1f, FurballGame.Time, FurballGame.Time + time));

        this._scoreMultiplier.Tweens.Add(new FloatTween(TweenType.Fade, this._scoreMultiplier.ColorOverride.Af, 1f,    FurballGame.Time, FurballGame.Time + time));
        this._background.Tweens.Add(new FloatTween(TweenType.Fade,      this._background.ColorOverride.Af,      0.75f, FurballGame.Time, FurballGame.Time + time));
    }

    public ModSelectionMenuDrawable(Vector2 pos) {
        this.Position = pos;

        int   lineAmounts = (int)Math.Ceiling(PlayerMod.RegisteredMods.Count / 5d);
        float lineY       = 0;
        for (int i = 0; i < lineAmounts; i++) {
            LinePrimitiveDrawable line;
            this.Drawables.Add(line = new LinePrimitiveDrawable(new(0, lineY + 1), new(406, 0), Color.White, 2f));
            this.Lines.Add(line);
            this.Drawables.Add(line = new LinePrimitiveDrawable(new(0, lineY + 2), new(406, 0), Color.Gray, 2f));
            this.Lines.Add(line);
            lineY += 100;
        }

        float x = 75;
        float y = 0;
        for (int i = 0; i < PlayerMod.RegisteredMods.Count; i++) {
            PlayerMod         registeredMod = PlayerMod.RegisteredMods[i];
            ModButtonDrawable modButton     = new(registeredMod, new(x, y), this.OnModClick);

            this.Drawables.Add(modButton);
            this._mods.Add(modButton);

            x += 15 + modButton.Size.X;

            if (i != 0 && i % 4 == 0) {
                x =  75;
                y += 100;
            }
        }

        this.Drawables.Add(this._scoreMultiplier = new TextDrawable(new(0, y - 100), pTypingGame.JapaneseFontStroked, "", 30));

        this.UpdateScoreMultiplierText();

        this.Drawables.Add(
        this._background = new RectanglePrimitiveDrawable(new(-2, -66), new(410, this.Size.Y + 4), 2, true) {
            ColorOverride = new(50, 50, 50, 175),
            Depth         = -1f
        }
        );
    }

    public sealed override Vector2 Size => base.Size;
}