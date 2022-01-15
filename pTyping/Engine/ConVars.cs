using Furball.Engine.Engine;
using Furball.Engine.Engine.DevConsole;
using Furball.Engine.Engine.DevConsole.Types;
using pTyping.Graphics.Player;
using pTyping.Online;
using pTyping.Scores;

namespace pTyping.Engine;

public class ConVars : ConVarStore {
    public static FloatConVar  Volume        = new("sl_master_volume", 0.05f);
    public static FloatConVar  BackgroundDim = new("cl_background_dim", 0.5f);
    public static StringConVar Username      = new("username", "beyley");
    public static StringConVar Password      = new("password", "test");

    public static ConFunc LoadUTypingReplay = new LoadUTypingReplay();
    public static ConFunc Logout            = new Logout();
    public static ConFunc Login             = new Login();
    public static ConFunc SendMessage       = new SendMessage();

    /// <summary>
    ///     The time it takes the notes to go from the right side of the screen to the left
    ///     this weird calculation converts the UTyping value over to a 16/9 aspect ratio to maintain the speed
    /// </summary>
    public static DoubleConVar BaseApproachTime = new("cl_base_approach_time", 2000d / (4d / 3d) * (16d / 9d)) {
        Protected = true
    };
}

public class LoadUTypingReplay : ConFunc {
    public LoadUTypingReplay() : base("cl_load_utyping_replay") {}

    public override ConsoleResult Run(string[] consoleInput) {
        ScreenManager.ChangeScreen(new PlayerScreen(PlayerScore.LoadUTypingReplay(consoleInput[0])));

        return new ConsoleResult(ExecutionResult.Success, "Loaded UTyping replay!");
    }
}

public class LoadAutoReplay : ConFunc {
    public LoadAutoReplay() : base("cl_load_auto_replay") {}

    public override ConsoleResult Run(string[] consoleInput) {
        ScreenManager.ChangeScreen(new PlayerScreen(AutoReplayCreator.CreateReplay(pTypingGame.CurrentSong.Value)));

        return new ConsoleResult(ExecutionResult.Success, "Loaded auto replay!");
    }
}

public class Logout : ConFunc {
    public Logout() : base("sv_logout") {}

    public override ConsoleResult Run(string[] consoleInput) {
        pTypingGame.OnlineManager.Logout();

        if (pTypingGame.OnlineManager.State != ConnectionState.Disconnected)
            return new(ExecutionResult.Error, "Logout not successful!");

        return new(ExecutionResult.Success, "Logged out!");
    }
}

public class Login : ConFunc {

    public Login() : base("sv_login") {}

    public override ConsoleResult Run(string[] consoleInput) {
        ConVars.Username.Value = consoleInput[0];

        pTypingGame.OnlineManager.Login();

        return new(ExecutionResult.Success, "Logging in!");
    }
}

public class SendMessage : ConFunc {
    public SendMessage() : base("sv_send_message") {}
    public override ConsoleResult Run(string[] consoleInput) {
        if (pTypingGame.OnlineManager.State != ConnectionState.LoggedIn)
            return new(ExecutionResult.Warning, "You are not logged in!");

        pTypingGame.OnlineManager.SendMessage(consoleInput[0], consoleInput[1]);

        return new(ExecutionResult.Success, "Message sent!");
    }
}