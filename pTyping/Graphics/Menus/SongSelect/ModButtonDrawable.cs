using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using Kettu;
using pTyping.Engine;
using pTyping.Shared.Mods;

namespace pTyping.Graphics.Menus.SongSelect;

public class ModButtonDrawable : TexturedDrawable {
	public readonly  Mod                  Mod;
	private readonly Vector2              _originalPosition;
	private readonly Action<object, bool> _actionClick;
	private readonly List<Mod>            _selectedMods;

	public ModButtonDrawable(Mod mod, Vector2 position, Action<object, bool> onModClick, List<Mod> selectedMods) : base(null, position) {
		this.Mod = mod;

		this.Texture = ContentManager.LoadTextureFromFileCached($"mod-{mod.ShorthandName}.png", ContentSource.User);

		this.Scale          = new(0.125f);
		this.OriginType     = OriginType.Center;
		this.RotationOrigin = this.Size / 2f;

		this.ToolTip = mod.ToolTip;

		this._originalPosition = position;

		this._incompat = selectedMods.FirstOrDefault(x => x.IsIncompatible(this.Mod) || this.Mod.IsIncompatible(x));

		if (this._incompat != null) {
			this.ColorOverride = DISABLED_COLOR;
			this.Clickable     = false;
		}

		if (selectedMods.Contains(mod)) {
			position           += CLICKED_MOVE;
			this.Rotation      =  CLICKED_ROT;
			this.ColorOverride =  CLICKED_COLOR;
		}

		this.Position = position;

		this.OnClick += this.OnModClick;

		this._actionClick  = onModClick;
		this._selectedMods = selectedMods;
	}

	public void Hide(bool instant = false) {
		float time = instant ? 0 : 500;

		this.Tweens.Add(
			new VectorTween(TweenType.Movement, this.Position, this._originalPosition - new Vector2(300, 0), FurballGame.Time, FurballGame.Time + time, Easing.Out)
		);
		this.Tweens.Add(new FloatTween(TweenType.Fade, this.ColorOverride.Af, 0f, FurballGame.Time, FurballGame.Time         + time));
		this.Tweens.Add(new FloatTween(TweenType.Rotation, this.Rotation, -MathF.PI * 2f, FurballGame.Time, FurballGame.Time + time, Easing.Out));

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
		if (this._selectedMods.Contains(this.Mod)) {
			posOffset += CLICKED_MOVE;
			rot       =  CLICKED_ROT;
		}

		this.Tweens.Add(new VectorTween(TweenType.Movement, this.Position, this._originalPosition                    + posOffset, FurballGame.Time, FurballGame.Time + time, Easing.Out));
		this.Tweens.Add(new FloatTween(TweenType.Fade, this.ColorOverride.Af, 1f, FurballGame.Time, FurballGame.Time + time));
		this.Tweens.Add(new FloatTween(TweenType.Rotation, this.Rotation, rot, FurballGame.Time, FurballGame.Time    + time, Easing.Out));

		FurballGame.GameTimeScheduler.ScheduleMethod(
			_ => {
				if (this._incompat == null)
					this.Clickable = true;
			},
			FurballGame.Time + time
		);
	}

	private static readonly Color   CLICKED_COLOR  = new Color(210, 210, 255);
	private static readonly Color   DISABLED_COLOR = new Color(255, 180, 180);
	private const           float   CLICKED_ROT    = MathF.PI / 10;
	private static readonly Vector2 CLICKED_MOVE   = new Vector2(10, 0);

	private void OnModClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		const float tweenTime = 100;

		if (this._selectedMods.Contains(this.Mod)) {
			this.Tweens.Add(new VectorTween(TweenType.Movement, this.Position, this._originalPosition, FurballGame.Time, FurballGame.Time + tweenTime, Easing.Out));
			this.Tweens.Add(new FloatTween(TweenType.Rotation, this.Rotation, 0, FurballGame.Time, FurballGame.Time                       + tweenTime, Easing.Out));
			this.Tweens.Add(new ColorTween(TweenType.Color, this.ColorOverride, Color.White, FurballGame.Time, FurballGame.Time           + tweenTime));

			this._selectedMods.Remove(this.Mod);
		}
		else {
			this.Tweens.Add(
				new VectorTween(TweenType.Movement, this.Position, this._originalPosition + CLICKED_MOVE, FurballGame.Time, FurballGame.Time + tweenTime, Easing.Out)
			);
			this.Tweens.Add(new FloatTween(TweenType.Rotation, this.Rotation, CLICKED_ROT, FurballGame.Time, FurballGame.Time     + tweenTime, Easing.Out));
			this.Tweens.Add(new ColorTween(TweenType.Color, this.ColorOverride, CLICKED_COLOR, FurballGame.Time, FurballGame.Time + tweenTime));

			this._selectedMods.Add(this.Mod);
		}

		this._actionClick?.Invoke(this, this._selectedMods.Contains(this.Mod));
	}

	[Description("The mod which is causing the incompatibility")]
	private Mod _incompat;
	public void ModStateChange(ModButtonDrawable modButton, bool added) {
		if (added && (modButton.Mod.IsIncompatible(this.Mod) || this.Mod.IsIncompatible(modButton.Mod))) {
			this.Clickable = false;
			this.Tweens.Add(new ColorTween(TweenType.Color, this.ColorOverride, DISABLED_COLOR, FurballGame.Time, FurballGame.Time + 100));
			this._incompat = modButton.Mod;
			Logger.Log($"DISABLING FOR :{this.Mod.Name}", LoggerLevelModInfo.Instance);
		}
		else if (!added) {
			if (this._incompat == modButton.Mod) {
				this.Clickable = true;
				this._incompat = null;
				this.Tweens.Add(new ColorTween(TweenType.Color, this.ColorOverride, Color.White, FurballGame.Time, FurballGame.Time + 100));
				Logger.Log($"ENABLING FOR :{this.Mod.Name}", LoggerLevelModInfo.Instance);
			}
		}
	}
}
