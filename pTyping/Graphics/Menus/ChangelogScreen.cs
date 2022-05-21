using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Helpers;
using Silk.NET.Input;
using Color=Furball.Vixie.Backends.Shared.Color;

namespace pTyping.Graphics.Menus;

public class ChangeLogDrawable : CompositeDrawable {
    public ChangeLogDrawable() {
        this.Clickable   = false;
        this.CoverClicks = false;

        float y = 0;

        foreach (GitLogEntry entry in Program.GitLog) {
            ChangeLogEntryDrawable drawable = new(entry) {
                Position = new Vector2(0, y)
            };

            this.Drawables.Add(drawable);
            y += drawable.Size.Y + 5;
        }
    }

    public override void Update(double time) {
        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (ChangeLogEntryDrawable drawable in this.Drawables)
            drawable.Visible = drawable.RealRectangle.IntersectsWith(FurballGame.DisplayRect);

        base.Update(time);
    }

    private class ChangeLogEntryDrawable : CompositeDrawable {
        private readonly TextDrawable _summary;
        private readonly TextDrawable _bottomLine;

        public override Vector2 Size => new(1, 60);

        private readonly GitLogEntry _entry;

        private static string ToRelativeDate(DateTime oldTime) {
            TimeSpan oSpan        = DateTime.Now.Subtract(oldTime);
            double   TotalMinutes = oSpan.TotalMinutes;
            string   sSuffix      = " ago";
            if (TotalMinutes < 0.0) {
                TotalMinutes = Math.Abs(TotalMinutes);
                sSuffix      = " from now";
            }
            SortedList<double, Func<string>> aValue = new();
            aValue.Add(0.75,            () => "less than a minute");
            aValue.Add(1.5,             () => "about a minute");
            aValue.Add(45,              () => string.Format("{0} minutes", Math.Round(TotalMinutes)));
            aValue.Add(90,              () => "about an hour");
            aValue.Add(1440,            () => string.Format("about {0} hours", Math.Round(Math.Abs(oSpan.TotalHours))));// 60 * 24
            aValue.Add(2880,            () => "a day");                                                                 // 60 * 48
            aValue.Add(43200,           () => string.Format("{0} days", Math.Floor(Math.Abs(oSpan.TotalDays))));        // 60 * 24 * 30
            aValue.Add(86400,           () => "about a month");                                                         // 60 * 24 * 60
            aValue.Add(525600,          () => string.Format("{0} months", Math.Floor(Math.Abs(oSpan.TotalDays / 30)))); // 60 * 24 * 365 
            aValue.Add(1051200,         () => "about a year");                                                          // 60 * 24 * 365 * 2
            aValue.Add(double.MaxValue, () => string.Format("{0} years", Math.Floor(Math.Abs(oSpan.TotalDays / 365))));
            return aValue.First(n => TotalMinutes < n.Key).Value.Invoke() + sSuffix;
        }

        public ChangeLogEntryDrawable(GitLogEntry entry) {
            this._summary = new TextDrawable(new Vector2(0, 0), pTypingGame.JapaneseFont, entry.Message, 30) {
                Depth = 0f
            };

            this._bottomLine = new TextDrawable(
            new Vector2(0, 30),
            pTypingGame.JapaneseFont,
            $"{entry.Author.TrimEnd(';')} - {ToRelativeDate(entry.Date)} - {entry.Commit.Substring(0, 8)}",
            25
            ) {
                ToolTip = $"{entry.Date.ToShortDateString()} - {entry.Commit}",
                Depth   = 0f
            };

            this._summary.OnClick += this.OnClicked;
            // this._bottomLine.OnClick += this.OnClicked;

            this._summary.OnHover += this.OnHovered;
            // this._bottomLine.OnHover += this.OnHovered;

            this._summary.OnHoverLost += this.OnHoveredLost;
            // this._bottomLine.OnHoverLost += this.OnHoveredLost;

            this.Drawables.Add(this._bottomLine);
            this.Drawables.Add(this._summary);

            this._entry = entry;
        }

        private void OnHoveredLost(object sender, EventArgs e) {
            this._summary.Tweens.Clear();
            this._summary.FadeColor(Color.White, 100);
        }

        private void OnHovered(object sender, EventArgs e) {
            this._summary.Tweens.Clear();
            this._summary.FadeColor(new Color(100, 100, 255), 100);
        }

        public override void Dispose() {
            this._summary.OnClick    -= this.OnClicked;
            this._bottomLine.OnClick -= this.OnClicked;

            base.Dispose();
        }

        private void OnClicked(object sender, (MouseButton button, Point pos) tuple) {
            LinkHelper.OpenLink($"https://github.com/Beyley/pTyping/commit/{this._entry.Commit}");
        }
    }
}

public class ChangelogScreen : pScreen {
    public override string         Name                 => "Changelog";
    public override string         State                => "Browsing the changelog";
    public override string         Details              => "";
    public override bool           ForceSpeedReset      => true;
    public override float          BackgroundFadeAmount => 0.7f;
    public override MusicLoopState LoopState            => MusicLoopState.NewSong;
    public override ScreenType     ScreenType           => ScreenType.Menu;

    public float TargetScroll = 0;

    private ChangeLogDrawable _changeLogDrawable;

    public override void Initialize() {
        base.Initialize();

        this._changeLogDrawable = new ChangeLogDrawable {
            Position = new Vector2(10),
            Depth    = 0.5
        };

        #region Back button

        pTypingGame.LoadBackButtonTexture();

        TexturedDrawable backButton = new(pTypingGame.BackButtonTexture, new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
            OriginType = OriginType.BottomLeft,
            Scale      = pTypingGame.BackButtonScale,
            Depth      = 0
        };

        backButton.OnClick += delegate {
            pTypingGame.MenuClickSound.PlayNew();
            ScreenManager.ChangeScreen(new MenuScreen());
        };

        this.Manager.Add(backButton);

        #endregion

        #region Background image

        this.Manager.Add(pTypingGame.CurrentSongBackground);

        #endregion

        FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;

        this.Manager.Add(this._changeLogDrawable);
    }

    public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Listening;

    private void OnMouseScroll(object sender, ((int scrollWheelId, float scrollAmount) scroll, string cursorName) e) {
        this.TargetScroll += e.scroll.scrollAmount;
    }

    public override void Update(double gameTime) {
        if (this.TargetScroll > 0)
            this.TargetScroll *= (float)(0.99 * gameTime * 1000);
        if (this.TargetScroll < -this._changeLogDrawable.Size.Y + FurballGame.DEFAULT_WINDOW_HEIGHT - 10) {
            float target = -this._changeLogDrawable.Size.Y + FurballGame.DEFAULT_WINDOW_HEIGHT - 10;

            float difference = Math.Abs(this.TargetScroll - target);

            difference *= 1f - (float)(0.99 * gameTime * 1000);

            this.TargetScroll += difference;
        }

        Vector2 adjustedPos = this._changeLogDrawable.Position - new Vector2(10);

        float y = this._changeLogDrawable.Position.Y;
        y += (float)((this.TargetScroll - adjustedPos.Y) / 200 * gameTime);

        this._changeLogDrawable.Position = new Vector2(10, y);

        base.Update(gameTime);
    }
}