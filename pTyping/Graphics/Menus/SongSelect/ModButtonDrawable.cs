using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Player.Mods;
using Silk.NET.Input;
using Color=Furball.Vixie.Backends.Shared.Color;

namespace pTyping.Graphics.Menus.SongSelect;

public class ModButtonDrawable : TexturedDrawable {
    public readonly  PlayerMod            Mod;
    private readonly Vector2              _originalPosition;
    private readonly Action<object, bool> _actionClick;

    private static readonly Dictionary<string, Texture> TEXTURE_CACHE = new();

    public ModButtonDrawable(PlayerMod mod, Vector2 position, Action<object, bool> onModClick) : base(null, position) {
        this.Mod = mod;

        if (TEXTURE_CACHE.TryGetValue(mod.IconFilename(), out Texture tex))
            this._texture = tex;
        else
            TEXTURE_CACHE.Add(mod.IconFilename(), this._texture = ContentManager.LoadTextureFromFile(mod.IconFilename(), ContentSource.User));

        this.Scale          = new(0.125f);
        this.OriginType     = OriginType.Center;
        this.RotationOrigin = this.Size / 2f;

        this.ToolTip = mod.ToolTip();

        this._originalPosition = position;

        this.incompat = pTypingGame.IsModIncompatible(mod);

        if (this.incompat != null) {
            this.ColorOverride = DISABLED_COLOR;
            this.Clickable     = false;
        }

        if (pTypingGame.SelectedMods.Contains(mod)) {
            position           += CLICKED_MOVE;
            this.Rotation      =  CLICKED_ROT;
            this.ColorOverride =  CLICKED_COLOR;
        }

        this.Position = position;

        this.OnClick += this.OnModClick;

        this._actionClick = onModClick;
    }

    public void Hide(bool instant = false) {
        float time = instant ? 0 : 500;

        this.Tweens.Add(
        new VectorTween(TweenType.Movement, this.Position, this._originalPosition - new Vector2(300, 0), FurballGame.Time, FurballGame.Time + time, Easing.Out)
        );
        this.Tweens.Add(new FloatTween(TweenType.Fade,     this.ColorOverride.Af, 0f,             FurballGame.Time, FurballGame.Time + time));
        this.Tweens.Add(new FloatTween(TweenType.Rotation, this.Rotation,         -MathF.PI * 2f, FurballGame.Time, FurballGame.Time + time, Easing.Out));

        FurballGame.GameTimeScheduler.ScheduleMethod(
        _ => {
            this.Clickable = false;
        },
        FurballGame.Time + time
        );
    }

    public void Show() {
        const float time = 500;

        Vector2 posOffset = Vector2.Zero;
        float   rot       = 0;
        if (pTypingGame.SelectedMods.Contains(this.Mod)) {
            posOffset += CLICKED_MOVE;
            rot       =  CLICKED_ROT;
        }

        this.Tweens.Add(new VectorTween(TweenType.Movement, this.Position, this._originalPosition + posOffset, FurballGame.Time, FurballGame.Time + time, Easing.Out));
        this.Tweens.Add(new FloatTween(TweenType.Fade,     this.ColorOverride.Af, 1f,  FurballGame.Time, FurballGame.Time + time));
        this.Tweens.Add(new FloatTween(TweenType.Rotation, this.Rotation,         rot, FurballGame.Time, FurballGame.Time + time, Easing.Out));

        FurballGame.GameTimeScheduler.ScheduleMethod(
        _ => {
            if (this.incompat == null)
                this.Clickable = true;
        },
        FurballGame.Time + time
        );
    }

    private static readonly Color   CLICKED_COLOR  = new(210, 210, 255);
    private static readonly Color   DISABLED_COLOR = new(255, 180, 180);
    private const           float   CLICKED_ROT    = MathF.PI / 10;
    private static readonly Vector2 CLICKED_MOVE   = new(10, 0);

    private void OnModClick(object sender, (MouseButton button, Point pos) e) {
        const float tweenTime = 100;

        if (pTypingGame.SelectedMods.Contains(this.Mod)) {
            this.Tweens.Add(new VectorTween(TweenType.Movement, this.Position, this._originalPosition, FurballGame.Time, FurballGame.Time + tweenTime, Easing.Out));
            this.Tweens.Add(new FloatTween(TweenType.Rotation, this.Rotation, 0, FurballGame.Time, FurballGame.Time                       + tweenTime, Easing.Out));
            this.Tweens.Add(new ColorTween(TweenType.Color, this.ColorOverride, Color.White, FurballGame.Time, FurballGame.Time           + tweenTime));

            pTypingGame.SelectedMods.Remove(this.Mod);
        } else {
            this.Tweens.Add(
            new VectorTween(TweenType.Movement, this.Position, this._originalPosition + CLICKED_MOVE, FurballGame.Time, FurballGame.Time + tweenTime, Easing.Out)
            );
            this.Tweens.Add(new FloatTween(TweenType.Rotation, this.Rotation, CLICKED_ROT, FurballGame.Time, FurballGame.Time     + tweenTime, Easing.Out));
            this.Tweens.Add(new ColorTween(TweenType.Color, this.ColorOverride, CLICKED_COLOR, FurballGame.Time, FurballGame.Time + tweenTime));

            pTypingGame.SelectedMods.Add(this.Mod);
        }

        this._actionClick?.Invoke(this, pTypingGame.SelectedMods.Contains(this.Mod));
    }

    private PlayerMod incompat;
    public void ModStateChange(ModButtonDrawable modButton, bool added) {
        if (added && (modButton.Mod.IncompatibleMods().Contains(this.Mod.GetType()) || this.Mod.IncompatibleMods().Contains(modButton.Mod.GetType()))) {
            this.Clickable = false;
            this.Tweens.Add(new ColorTween(TweenType.Color, this.ColorOverride, DISABLED_COLOR, FurballGame.Time, FurballGame.Time + 100));
            this.incompat = modButton.Mod;
            Console.WriteLine($"DISABLING FOR :{this.Mod.Name()}");
        } else if (!added) {
            if (this.incompat == modButton.Mod) {
                this.Clickable = true;
                this.incompat  = null;
                this.Tweens.Add(new ColorTween(TweenType.Color, this.ColorOverride, Color.White, FurballGame.Time, FurballGame.Time + 100));
                Console.WriteLine($"ENABLING FOR :{this.Mod.Name()}");
            }
        }
    }
}
