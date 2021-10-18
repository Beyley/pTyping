using pTyping.Online;
using Furball.Engine.Engine.DevConsole;
using Furball.Engine.Engine.DevConsole.Types;

namespace pTyping {
    public class ConVars {
        public static SendMessage SendMessage = new();
        public static Login       Login       = new();
        public static Logout      Logout      = new();

        public static FloatConVar  Volume        = new("sl_master_volume", 0.05f);
        public static FloatConVar  BackgroundDim = new("cl_background_dim", 0.5f);
        public static StringConVar Username      = new("username", "Guest");
        public static StringConVar Password      = new("password", "password");

        /// <summary>
        /// The time it takes the notes to go from the right side of the screen to the left 
        /// </summary>
        public static IntConVar BaseApproachTime = new("cl_base_approach_time", 2000);
    }

    public class Logout : ConFunc {
        public Logout() : base("net_logout") {}

        public override ConsoleResult Run(string consoleInput) {
            pTypingGame.OnlineManager.Logout().Wait();

            if (pTypingGame.OnlineManager.State != ConnectionState.Disconnected)
                return new(ExecutionResult.Error, "Logout not successful!");

            return new(ExecutionResult.Success, "Logged out!");
        }
    }

    public class Login : ConFunc {

        public Login() : base("net_login") {}

        public override ConsoleResult Run(string consoleInput) {
            pTypingGame.OnlineManager.Login().Wait();

            if (pTypingGame.OnlineManager.State != ConnectionState.LoggedIn)
                return new(ExecutionResult.Error, "Login not successful!");

            return new(ExecutionResult.Success, "Logged in!");
        }
    }

    public class SendMessage : ConFunc {
        public SendMessage() : base("net_send_message") {}
        public override ConsoleResult Run(string consoleInput) {
            if (pTypingGame.OnlineManager.State != ConnectionState.LoggedIn)
                return new(ExecutionResult.Warning, "You are not logged in!");

            string[] split = consoleInput.Split(" ");

            pTypingGame.OnlineManager.SendMessage(split[0], string.Join(" ", split, 1, split.Length - 1)).Wait();

            return new(ExecutionResult.Success, "Message sent!");
        }
    }
}
