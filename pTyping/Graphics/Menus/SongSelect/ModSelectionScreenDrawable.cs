using System;
using System.Collections.Generic;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Player.Mods;

namespace pTyping.Graphics.Menus.SongSelect;

public class ModSelectionScreenDrawable : CompositeDrawable {
    private readonly List<ModButtonDrawable> _mods = new();

    private readonly Color _unselectedColor = Color.Red;
    private readonly Color _selectedColor   = Color.Blue;

    // private readonly TextDrawable     _scoreMultiplier;

    private readonly LinePrimitiveDrawable line;
    // private          bool             test2;

    public event EventHandler OnModAdd;

    public void OnModClick(object sender, bool added) {
        ModButtonDrawable modButton = (ModButtonDrawable)sender;

        foreach (ModButtonDrawable modButtonDrawable in this._mods)
            if (modButton != modButtonDrawable)
                modButtonDrawable.ModStateChange(modButton, added);
    }
    
    public ModSelectionScreenDrawable(Vector2 pos) {
        this.Position = pos;

        int   lineAmounts = (int)Math.Ceiling(PlayerMod.RegisteredMods.Count / 5d);
        float lineY       = 0;
        for (int i = 0; i < lineAmounts; i++) {
            this.Drawables.Add(this.line = new LinePrimitiveDrawable(new(0, lineY + 1), new(406, 0), Color.White, 2f));
            this.Drawables.Add(this.line = new LinePrimitiveDrawable(new(0, lineY + 2), new(406, 0), Color.Gray,  2f));
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
    }

    private void OnButtonClick(DrawableButton modButton, PlayerMod mod) {
        // if (pTypingGame.SelectedMods.Contains(mod)) {
        //     pTypingGame.SelectedMods.Remove(mod);
        //     modButton.ButtonColor = this._unselectedColor;
        //     modButton.FadeColor(this._unselectedColor, 100);
        // } else {
        //     for (int i = 0; i < this._mods.Count; i++) {
        //         (PlayerMod playerMod, DrawableButton button) = this._mods[i];
        //
        //         if (mod.IncompatibleMods().Contains(playerMod.GetType()))
        //             if (pTypingGame.SelectedMods.Contains(playerMod)) {
        //                 button.ButtonColor = this._unselectedColor;
        //                 button.FadeColor(this._unselectedColor, 100);
        //                 pTypingGame.SelectedMods.Remove(playerMod);
        //             }
        //     }
        //
        //     pTypingGame.SelectedMods.Add(mod);
        //     modButton.ButtonColor = this._selectedColor;
        //     modButton.FadeColor(this._selectedColor, 100);
        // }

        this.OnModAdd?.Invoke(this, EventArgs.Empty);

        // this._scoreMultiplier.Text = $"Score Multiplier: {PlayerMod.ScoreMultiplier(pTypingGame.SelectedMods):#0.##}x";
    }
}