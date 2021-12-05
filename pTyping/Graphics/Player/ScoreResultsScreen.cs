using System;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Graphics.Player.Mods;
using pTyping.Scores;

namespace pTyping.Graphics.Player {
    public class ScoreResultsScreen : pScreen {
        public PlayerScore Score;

        public ScoreResultsScreen(PlayerScore score) => this.Score = score;

        public override void Initialize() {
            base.Initialize();

            #region Title

            TextDrawable songTitleText = new(
            new(10, 10),
            pTypingGame.JapaneseFont,
            $"{pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}]",
            40
            );
            TextDrawable songCreatorText = new(
            new(10, songTitleText.Size.Y + 20),
            pTypingGame.JapaneseFont,
            $"Created by {pTypingGame.CurrentSong.Value.Creator}",
            30
            );

            this.Manager.Add(songTitleText);
            this.Manager.Add(songCreatorText);

            #endregion

            #region Score info

            float y = 175;

            TextDrawable score = new(new(100, y), FurballGame.DEFAULT_FONT, $"Score: {this.Score.Score:00000000}", 35);
            y += 20 + score.Size.Y;
            TextDrawable accuracy = new(new(100, y), FurballGame.DEFAULT_FONT, $"Accuracy: {this.Score.Accuracy * 100:0.00}%", 35);
            y += 20 + accuracy.Size.Y;
            TextDrawable combo = new(new(100, y), FurballGame.DEFAULT_FONT, $"Combo: {this.Score.MaxCombo}x", 35);
            y += 20 + combo.Size.Y;
            TextDrawable excellent = new(new(100, y), FurballGame.DEFAULT_FONT, $"Excellent: {this.Score.ExcellentHits}x", 35);
            y += 20 + excellent.Size.Y;
            TextDrawable good = new(new(100, y), FurballGame.DEFAULT_FONT, $"Good: {this.Score.GoodHits}x", 35);
            y += 20 + good.Size.Y;
            TextDrawable fair = new(new(100, y), FurballGame.DEFAULT_FONT, $"Fair: {this.Score.FairHits}x", 35);
            y += 20 + fair.Size.Y;
            TextDrawable poor = new(new(100, y), FurballGame.DEFAULT_FONT, $"Poor/Miss: {this.Score.PoorHits}x", 35);
            y += 20 + poor.Size.Y;
            // TextDrawable miss = new(new(100, y), FurballGame.DEFAULT_FONT, $"Miss: {this.Score.MissHits}x", 35);
            // y += 20 + miss.Size.Y;
            TextDrawable mods = new(new(100, y), FurballGame.DEFAULT_FONT, $"Mods: {PlayerMod.GetModString(this.Score.Mods)}", 35);

            this.Manager.Add(score);
            this.Manager.Add(accuracy);
            this.Manager.Add(combo);
            this.Manager.Add(excellent);
            this.Manager.Add(good);
            this.Manager.Add(fair);
            this.Manager.Add(poor);
            // this.Manager.Add(miss);
            this.Manager.Add(mods);

            #endregion

            #region Buttons

            EventHandler<Point> exitOnClick = delegate {
                pTypingGame.MenuClickSound.Play();
                ScreenManager.ChangeScreen(new SongSelectionScreen(false));
            };

            UiButtonDrawable exitButton = new(
            new(FurballGame.DEFAULT_WINDOW_WIDTH - 20f, FurballGame.DEFAULT_WINDOW_HEIGHT - 20f),
            "Exit",
            FurballGame.DEFAULT_FONT,
            40,
            Color.Red,
            Color.White,
            Color.White,
            Vector2.Zero,
            exitOnClick
            ) {
                OriginType = OriginType.BottomRight
            };

            this.Manager.Add(exitButton);

            #endregion

            #region background image

            this.Manager.Add(pTypingGame.CurrentSongBackground);

            pTypingGame.CurrentSongBackground.Tweens.Add(
            new ColorTween(
            TweenType.Color,
            pTypingGame.CurrentSongBackground.ColorOverride,
            new(0.5f, 0.5f, 0.5f),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000
            )
            );
            pTypingGame.LoadBackgroundFromSong(pTypingGame.CurrentSong.Value);

            #endregion

            if (pTypingGame.MusicTrack.IsValidHandle)
                pTypingGame.MusicTrack.AudioRate = 1f;

            pTypingGame.UserStatusListening();
        }

        public override string Name  => "Score Results";
        public override string State => "Looking at scores!";

        public override string Details => $@"{pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}]
Played by {this.Score.Username}
Score: {this.Score.Score:0000000} Accuracy: {100d * this.Score.Accuracy:00.##}%";
    }
}
