using System.Collections.Generic;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;
using pTyping.Player.Mods;

namespace pTyping.Drawables {
    public class ModSelectionScreenDrawable : CompositeDrawable {
        private readonly List<(PlayerMod mod, UiButtonDrawable button)> _mods = new();

        private readonly Color _unselectedColor = Color.Red;
        private readonly Color _selectedColor   = Color.Blue;

        public ModSelectionScreenDrawable(Vector2 pos) {
            this.Position = pos;

            float x = 0;
            for (int i = 0; i < PlayerMod.RegisteredMods.Count; i++) {
                PlayerMod mod = PlayerMod.RegisteredMods[i];

                UiButtonDrawable modButton = new(
                new(x, 0),
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
            }
        }

        private void OnButtonClick(UiButtonDrawable modButton, PlayerMod mod) {
            if (pTypingGame.SelectedMods.Contains(mod)) {
                pTypingGame.SelectedMods.Remove(mod);
                modButton.FadeColor(this._unselectedColor, 100);
            } else {
                for (int i = 0; i < this._mods.Count; i++) {
                    (PlayerMod playerMod, UiButtonDrawable button) = this._mods[i];

                    if (mod.IncompatibleMods().Contains(playerMod.GetType()))
                        if (pTypingGame.SelectedMods.Contains(playerMod)) {
                            button.FadeColor(this._unselectedColor, 100);
                            pTypingGame.SelectedMods.Remove(playerMod);
                        }
                }

                pTypingGame.SelectedMods.Add(mod);
                modButton.FadeColor(this._selectedColor, 100);
            }
        }
    }
}
