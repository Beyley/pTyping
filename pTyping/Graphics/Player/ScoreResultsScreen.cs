using System;
using System.Drawing;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Scores;
using Silk.NET.Input;
using Color=Furball.Vixie.Backends.Shared.Color;

namespace pTyping.Graphics.Player;

public class ScoreResultsScreen : pScreen {
    public PlayerScore Score;

    public ScoreResultsScreen(PlayerScore score) => this.Score = score;

    public override void Initialize() {
        base.Initialize();

        #region Title

        TextDrawable songTitleText = new(
        new Vector2(10, 10),
        pTypingGame.JapaneseFont,
        $"{pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}]",
        40
        );
        TextDrawable songCreatorText = new(new Vector2(10, songTitleText.Size.Y + 20), pTypingGame.JapaneseFont, $"Created by {pTypingGame.CurrentSong.Value.Creator}", 30);

        this.Manager.Add(songTitleText);
        this.Manager.Add(songCreatorText);

        #endregion

        #region Score info

        ScoreResultsDrawable playerResults = new(this.Score) {
            Position = new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f)
        };
        
        this.Manager.Add(playerResults);
        
        #endregion

        #region Buttons

        EventHandler<(MouseButton, Point)> exitOnClick = delegate {
            pTypingGame.MenuClickSound.PlayNew();
            ScreenManager.ChangeScreen(new SongSelectionScreen(false));
        };

        UiButtonDrawable exitButton = new(
        new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 20f, FurballGame.DEFAULT_WINDOW_HEIGHT - 20f),
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

        EventHandler<(MouseButton, Point)> watchReplayOnClick = delegate {
            pTypingGame.MenuClickSound.PlayNew();
            ScreenManager.ChangeScreen(new PlayerScreen(this.Score));
        };

        UiButtonDrawable watchReplayButton = new(
        new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 20f, FurballGame.DEFAULT_WINDOW_HEIGHT - 80f),
        "Watch Replay",
        FurballGame.DEFAULT_FONT,
        40,
        Color.Blue,
        Color.White,
        Color.White,
        Vector2.Zero,
        watchReplayOnClick
        ) {
            OriginType = OriginType.BottomRight
        };

        this.Manager.Add(watchReplayButton);

        #endregion

        #region background image

        this.Manager.Add(pTypingGame.CurrentSongBackground);

        #endregion

        pTypingGame.UserStatusListening();
    }

    public override string Name  => "Score Results";
    public override string State => "Looking at scores!";

    public override string Details => $@"{pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}]
Played by {this.Score.Username}
Score: {this.Score.Score:0000000} Accuracy: {100d * this.Score.Accuracy:00.##}%";
    public override bool           ForceSpeedReset      => false;
    public override float          BackgroundFadeAmount => 0.5f;
    public override MusicLoopState LoopState            => MusicLoopState.None;
    public override ScreenType     ScreenType           => ScreenType.Menu;
}