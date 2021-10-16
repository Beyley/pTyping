using pTyping.Online;
using Furball.Engine.Engine.Console;
using Furball.Engine.Engine.Console.Types;

namespace pTyping {
    public class ConVars {
        public static SendMessage SendMessage = new();
        public static Login       Login       = new();
        public static Logout      Logout      = new();

        public static FloatConVar  Volume        = new("master_volume", 0.05f);
        public static FloatConVar  BackgroundDim = new("background_dim", 0.5f);
        public static StringConVar Username      = new("username", "Guest");
        public static StringConVar Password      = new("password", "password");

        /// <summary>
        /// The time it takes the notes to go from the right side of the screen to the left 
        /// </summary>
        public static IntConVar BaseApproachTime = new("base_approach_time", 2000);
    }

    public class Logout : ConFunc {
        public Logout() : base("logout") {}

        public override string Run(string consoleInput) {
            pTypingGame.OnlineManager.Logout().Wait();

            if (pTypingGame.OnlineManager.State != ConnectionState.Disconnected)
                return "Logout not successful!";

            return "Logged out!";
        }
    }

    public class Login : ConFunc {

        public Login() : base("login") {}

        public override string Run(string consoleInput) {
            pTypingGame.OnlineManager.Login().Wait();

            if (pTypingGame.OnlineManager.State != ConnectionState.LoggedIn)
                return "Login not successful!";

            return "Logged in!";
        }
    }

    public class SendMessage : ConFunc {
        public SendMessage() : base("send_message") {}
        public override string Run(string consoleInput) {
            if (pTypingGame.OnlineManager.State != ConnectionState.LoggedIn)
                return "You are not logged in!";

            string[] split = consoleInput.Split(" ");

            pTypingGame.OnlineManager.SendMessage(split[0], string.Join(" ", split, 1, split.Length - 1)).Wait();

            return "Message sent!";
        }
    }
}
