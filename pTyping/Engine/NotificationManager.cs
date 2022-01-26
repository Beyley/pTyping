using System;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Input;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace pTyping.Engine;

public class NotificationManager : DrawableManager {
    public enum NotificationImportance : byte {
        Info    = 0,
        Warning = 1,
        Error   = 2
    }

    public NotificationDrawable CreateNotification(NotificationImportance importance, string text) {
        NotificationDrawable drawable = new(importance, text) {
            OriginType = OriginType.BottomRight,
            Position   = new(FurballGame.DEFAULT_WINDOW_WIDTH + 500, FurballGame.DEFAULT_WINDOW_HEIGHT + 500)
        };
        drawable.OnClick += this.OnDrawableClick;
        drawable.FadeInFromZero(100);

        this.Add(drawable);
        this.UpdateNotifications();

        return drawable;
    }

    private void OnDrawableClick(object? sender, (Point pos, MouseButton button) valueTuple) {
        if (sender is not NotificationDrawable drawable)
            return;

        drawable.OnNotificationClick?.Invoke();

        this.RemoveDrawable(drawable);
    }

    public override void Update(GameTime time) {
        for (int i = 0; i < this.Drawables.Count; i++) {
            BaseDrawable baseDrawable = this.Drawables[i];
            if (baseDrawable is not NotificationDrawable drawable || drawable.ScheduledForRemoval)
                continue;

            if (drawable.TimeSource.GetCurrentTime() > drawable.StartTime + drawable.Duration)
                this.RemoveDrawable(drawable);
        }

        base.Update(time);
    }

    private void RemoveDrawable(NotificationDrawable drawable) {
        if (drawable.ScheduledForRemoval) return;

        drawable.ScheduledForRemoval = true;

        drawable.FadeOutFromOne(100);

        FurballGame.GameTimeScheduler.ScheduleMethod(
        delegate {
            this.Remove(drawable);
            this.UpdateNotifications();
        },
        drawable.TimeSource.GetCurrentTime() + 100
        );
    }

    public void UpdateNotifications() {
        const float x = FurballGame.DEFAULT_WINDOW_WIDTH - 10;
        float       y = FurballGame.DEFAULT_WINDOW_HEIGHT - 10;

        foreach (BaseDrawable baseDrawable in this.Drawables) {
            if (baseDrawable is not NotificationDrawable drawable) continue;

            drawable.MoveTo(new(x, y), 100);

            y -= drawable.Size.Y + 10;
        }
    }

    public class NotificationDrawable : CompositeDrawable {
        public bool ScheduledForRemoval;

        public override Vector2 Size => this._backgroundDrawable.Size;

        private readonly TextDrawable               _textDrawable;
        private readonly RectanglePrimitiveDrawable _backgroundDrawable;
        private readonly RectanglePrimitiveDrawable _outlineDrawable;
        // private RectanglePrimitiveDrawable _outlineDrawable2;
        private NotificationImportance _importance;

        public double StartTime;
        public double Duration;

        [CanBeNull]
        public Func<bool> OnNotificationClick;

        public NotificationDrawable(NotificationImportance importance, string text) {
            this._importance = importance;

            this._textDrawable = new(new(5f), pTypingGame.JapaneseFontStroked, text, 20) {
                Clickable   = false,
                CoverClicks = false,
                Hoverable   = false,
                CoverHovers = false
            };
            this._backgroundDrawable = new(Vector2.Zero, this._textDrawable.Size + new Vector2(10f), 0, true) {
                ColorOverride = new(50, 50, 50, 100),
                Clickable     = false,
                CoverClicks   = false,
                Hoverable     = false,
                CoverHovers   = false
            };
            this._outlineDrawable = new(Vector2.Zero, this._textDrawable.Size + new Vector2(10f), 2, false) {
                ColorOverride = importance switch {
                    NotificationImportance.Info    => new(225, 225, 225, 200),
                    NotificationImportance.Warning => Color.Yellow,
                    NotificationImportance.Error   => Color.Red,
                    _                              => throw new ArgumentOutOfRangeException(nameof (importance), importance, "huh? the fuck did you just do?")
                },
                Clickable   = false,
                CoverClicks = false,
                Hoverable   = false,
                CoverHovers = false
            };

            this.StartTime = this.TimeSource.GetCurrentTime();

            this.Duration = importance switch {
                NotificationImportance.Info    => 10000,
                NotificationImportance.Warning => 20000,
                NotificationImportance.Error   => 30000,
                _                              => throw new ArgumentOutOfRangeException(nameof (importance), importance, "what the hell?")
            };

            this._drawables.Add(this._backgroundDrawable);
            this._drawables.Add(this._outlineDrawable);
            this._drawables.Add(this._textDrawable);
        }
    }
}