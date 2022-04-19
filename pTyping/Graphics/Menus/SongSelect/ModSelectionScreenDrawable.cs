using System.Collections.Generic;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Player.Mods;

namespace pTyping.Graphics.Menus.SongSelect;

public class ModSelectionScreenDrawable : CompositeDrawable {
    private readonly List<(PlayerMod mod, UiButtonDrawable button)> _mods = new();

    private readonly Color _unselectedColor = Color.Red;
    private readonly Color _selectedColor   = Color.Blue;

    private readonly TextDrawable _scoreMultiplier;

    public ModSelectionScreenDrawable(Vector2 pos) {
        this.Position = pos;

        float x = 0;
        float y = 0;
        for (int i = 0; i < PlayerMod.RegisteredMods.Count; i++) {
            PlayerMod mod = PlayerMod.RegisteredMods[i];

            UiButtonDrawable modButton = new(
            new(x, y),
            mod.Name(),
            FurballGame.DEFAULT_FONT_STROKED,
            30,
            pTypingGame.SelectedMods.Contains(mod) ? this._selectedColor : this._unselectedColor,
            Color.White,
            Color.White,
            new(0)
            );

            modButton.OnClick += delegate {
                this.OnButtonClick(modButton, mod);
            };

            this._mods.Add((mod, modButton));
            this.Drawables.Add(modButton);

            x += modButton.Size.X + 30;
            if ((i + 1) % 4 == 0 && i != PlayerMod.RegisteredMods.Count - 1) {
                x =  0;
                y += modButton.Size.Y + 25;
            }

            if (i == PlayerMod.RegisteredMods.Count - 1)
                y += modButton.Size.Y + 25;
        }

        this._scoreMultiplier = new(
        new(0, y + 10),
        FurballGame.DEFAULT_FONT_STROKED,
        $"Score Multiplier: {PlayerMod.ScoreMultiplier(pTypingGame.SelectedMods):#0.##}x",
        30
        );
        this.Drawables.Add(this._scoreMultiplier);
    }

    private void OnButtonClick(UiButtonDrawable modButton, PlayerMod mod) {
        if (pTypingGame.SelectedMods.Contains(mod)) {
            pTypingGame.SelectedMods.Remove(mod);
            modButton.ButtonColor = this._unselectedColor;
            modButton.FadeColor(this._unselectedColor, 100);
        } else {
            for (int i = 0; i < this._mods.Count; i++) {
                (PlayerMod playerMod, UiButtonDrawable button) = this._mods[i];

                if (mod.IncompatibleMods().Contains(playerMod.GetType()))
                    if (pTypingGame.SelectedMods.Contains(playerMod)) {
                        button.ButtonColor = this._unselectedColor;
                        button.FadeColor(this._unselectedColor, 100);
                        pTypingGame.SelectedMods.Remove(playerMod);
                    }
            }

            pTypingGame.SelectedMods.Add(mod);
            modButton.ButtonColor = this._selectedColor;
            modButton.FadeColor(this._selectedColor, 100);
        }

        this._scoreMultiplier.Text = $"Score Multiplier: {PlayerMod.ScoreMultiplier(pTypingGame.SelectedMods):#0.##}x";
    }
}