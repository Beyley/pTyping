using System;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using JetBrains.Annotations;

namespace pTyping.Engine;

public class NotificationManager : DrawableManager {
	public enum NotificationImportance : byte {
		Info    = 0,
		Warning = 1,
		Error   = 2
	}

	public enum NotificationType : byte {
		BottomRight = 0,
		MiddlePopup = 1
	}

	public NotificationDrawable CreateNotification(NotificationImportance importance, string text) {
		NotificationDrawable drawable = new NotificationDrawable(importance, NotificationType.BottomRight, text) {
			OriginType       = OriginType.BottomRight,
			ScreenOriginType = OriginType.BottomRight,
			Position         = new Vector2(-500, -500)
		};
		drawable.OnClick += this.OnDrawableClick;
		drawable.FadeInFromZero(100);

		this.Add(drawable);
		this.UpdateNotifications();

		return drawable;
	}

	public NotificationDrawable CreatePopup(string text) {
		NotificationDrawable drawable = new NotificationDrawable(NotificationImportance.Info, NotificationType.MiddlePopup, text) {
			OriginType = OriginType.Center,
			Position   = new Vector2(FurballGame.WindowWidth / 2f, FurballGame.WindowHeight / 2f)
		};
		drawable.FadeInFromZero(100);

		this.UpdatePopups();
		this.Add(drawable);

		return drawable;
	}

	private void OnDrawableClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		if (sender is not NotificationDrawable drawable)
			return;

		drawable.OnNotificationClick?.Invoke();

		this.RemoveDrawable(drawable);
	}

	public override void Update(double time) {
		for (int i = 0; i < this.Drawables.Count; i++) {
			Drawable baseDrawable = this.Drawables[i];
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

	private void UpdatePopups() {
		foreach (Drawable baseDrawable in this.Drawables) {
			if (baseDrawable is not NotificationDrawable drawable) continue;

			if (drawable.Type == NotificationType.MiddlePopup && !drawable.ScheduledForRemoval) {
				drawable.FadeOutFromOne(100);
				drawable.ScheduledForRemoval = true;

				FurballGame.GameTimeScheduler.ScheduleMethod(
					delegate {
						this.Remove(drawable);
					},
					drawable.TimeSource.GetCurrentTime() + 100
				);
			}
		}
	}

	public void UpdateNotifications() {
		const float x = 10;
		float       y = 10;

		foreach (Drawable drawable in this.Drawables) {
			if (drawable is not NotificationDrawable {
					Type: NotificationType.BottomRight
				} notification) continue;

			notification.MoveTo(new Vector2(x, y), 100);

			y += notification.Size.Y + 10;
		}
	}

	public class NotificationDrawable : CompositeDrawable {
		public bool ScheduledForRemoval;

		public override Vector2 Size => this._backgroundDrawable.Size * this.Scale;

		private readonly TextDrawable               _textDrawable;
		private readonly RectanglePrimitiveDrawable _backgroundDrawable;
		private readonly RectanglePrimitiveDrawable _outlineDrawable;
		// private RectanglePrimitiveDrawable _outlineDrawable2;
		public          NotificationImportance _importance;
		public readonly NotificationType       Type;

		public double StartTime;
		public double Duration;

		[CanBeNull]
		public Func<bool> OnNotificationClick;

		public NotificationDrawable(NotificationImportance importance, NotificationType type, string text) {
			this._importance = importance;
			this.Type        = type;

			this._textDrawable = new TextDrawable(new Vector2(5f), pTypingGame.JapaneseFont, text, 20) {
				Clickable   = false,
				CoverClicks = false,
				Hoverable   = false,
				CoverHovers = false
			};
			this._backgroundDrawable = new RectanglePrimitiveDrawable(Vector2.Zero, this._textDrawable.Size + new Vector2(10f), 0, true) {
				ColorOverride = new Color(50, 50, 50, 100),
				Clickable     = false,
				CoverClicks   = false,
				Hoverable     = false,
				CoverHovers   = false
			};
			this._outlineDrawable = new RectanglePrimitiveDrawable(Vector2.Zero, this._textDrawable.Size + new Vector2(10f), 2, false) {
				ColorOverride = importance switch {
					NotificationImportance.Info    => new Color(225, 225, 225, 200),
					NotificationImportance.Warning => Color.Red,
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

			if (type == NotificationType.MiddlePopup) {
				this.Duration = 2000;

				this.Clickable   = false;
				this.CoverClicks = false;
				this.Hoverable   = false;
				this.CoverHovers = false;
			}

			this.Depth = -1;

			this.Drawables.Add(this._backgroundDrawable);
			this.Drawables.Add(this._outlineDrawable);
			this.Drawables.Add(this._textDrawable);
		}
	}
}
