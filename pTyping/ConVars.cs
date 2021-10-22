using Furball.Engine.Engine.DevConsole;
using Furball.Engine.Engine.DevConsole.Types;
using pTyping.Online;

namespace pTyping {
    public class ConVars : ConVarStore {
        public static FloatConVar  Volume        = new("sl_master_volume", 0.05f);
        public static FloatConVar  BackgroundDim = new("cl_background_dim", 0.5f);
        public static StringConVar Username      = new("username", "beyley");
        public static StringConVar Password = new(
        "password",
        "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff"
        );

        /// <summary>
        ///     The time it takes the notes to go from the right side of the screen to the left
        /// </summary>
        public static IntConVar BaseApproachTime = new("cl_base_approach_time", 2000);
    }

    public class Logout : ConFunc {
        public Logout() : base("sv_logout") {}

        public override ConsoleResult Run(string[] consoleInput) {
            pTypingGame.OnlineManager.Logout().Wait();

            if (pTypingGame.OnlineManager.State != ConnectionState.Disconnected)
                return new(ExecutionResult.Error, "Logout not successful!");

            return new(ExecutionResult.Success, "Logged out!");
        }
    }

    public class Login : ConFunc {

        public Login() : base("sv_login") {}

        public override ConsoleResult Run(string[] consoleInput) {
            pTypingGame.OnlineManager.Login().Wait();

            if (pTypingGame.OnlineManager.State != ConnectionState.LoggedIn)
                return new(ExecutionResult.Error, "Login not successful!");

            return new(ExecutionResult.Success, "Logged in!");
        }
    }

    public class SendMessage : ConFunc {
        public SendMessage() : base("sv_send_message") {}
        public override ConsoleResult Run(string[] consoleInput) {
            if (pTypingGame.OnlineManager.State != ConnectionState.LoggedIn)
                return new(ExecutionResult.Warning, "You are not logged in!");

            pTypingGame.OnlineManager.SendMessage(consoleInput[0], consoleInput[1]).Wait();

            return new(ExecutionResult.Success, "Message sent!");
        }
    }
}
