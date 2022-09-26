using System.Text;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Helpers;
using Furball.Volpe.Evaluation;
using Kettu;
using pTyping.Graphics.Player;
using pTyping.Scores;
using pTyping.Shared.Scores;

namespace pTyping.Engine;

#nullable enable
public class ConVars {
	public static TypedVariable<Value.Number> Volume        = new TypedVariable<Value.Number>("sl_master_volume", new Value.Number(0.05));
	public static TypedVariable<Value.Number> BackgroundDim = new TypedVariable<Value.Number>("cl_background_dim", new Value.Number(0.5));
	// public static TypedVariable<Value.String> Username      = new("net_username", new Value.String("beyley"));
	// public static TypedVariable<Value.String> Password      = new("net_password", new Value.String("test"));

	// public static FloatConVar  Volume        = new("sl_master_volume", 0.05f);
	// public static FloatConVar  BackgroundDim = new("cl_background_dim", 0.5f);
	// public static StringConVar Username      = new("net_username", "beyley");
	// public static StringConVar Password      = new("net_password", "test");
	/// <summary>
	///     The time it takes the notes to go from the right side of the screen to the left
	///     this weird calculation converts the UTyping value over to a 16/9 aspect ratio to maintain the speed
	/// </summary>
	public const double BASE_APPROACH_TIME = 2000d / (4d / 3d) * (16d / 9d);

	public static BuiltinFunction LoadUTypingReplay = new BuiltinFunction("cl_load_utyping_replay", 1, (context, parameters) => {
		if (parameters[0] is not Value.String filename)
			return Value.DefaultVoid;

		Logger.Log($"Loading UTyping replay {parameters[0].Representation}", LoggerLevelPlayerInfo.Instance);
		ScreenManager.ChangeScreen(new PlayerScreen(ScoreExtensions.LoadUTypingReplay(filename.Value)));

		return Value.DefaultVoid;
	});
	public static BuiltinFunction LoadAutoReplay = new BuiltinFunction("cl_load_auto_replay", 0, (context, parameters) => {
		Logger.Log("Loading Auto replay", LoggerLevelPlayerInfo.Instance);

		Score playerScore = AutoReplayCreator.CreateReplay(pTypingGame.CurrentSong.Value);

		ScreenManager.ChangeScreen(new PlayerScreen(playerScore));

		return Value.DefaultVoid;
	});
	public static BuiltinFunction Logout = new BuiltinFunction("sv_logout", 0, (context, parameters) => {
		pTypingGame.OnlineManager.Logout();

		return Value.DefaultVoid;
	});
	public static BuiltinFunction Login = new BuiltinFunction("sv_login", 2, (context, parameters) => {
		if (parameters[0] is not Value.String username || parameters[1] is not Value.String password)
			return Value.DefaultVoid;

		pTypingConfig.Instance.Values["username"] = new Value.String(username.Value);
		pTypingConfig.Instance.Values["password"] = new Value.String(CryptoHelper.GetSha512(Encoding.UTF8.GetBytes(password.Value)));

		pTypingGame.OnlineManager.Login();

		return Value.DefaultVoid;
	});
	public static BuiltinFunction SendMessage = new BuiltinFunction("sv_send_message", 2, (context, parameters) => {
		if (parameters[0] is not Value.String channel || parameters[1] is not Value.String message)
			return Value.DefaultVoid;

		pTypingGame.OnlineManager.SendMessage(channel.Value, message.Value);

		return Value.DefaultVoid;
	});
}
//
// public class LoadUTypingReplay : ConFunc {
//     public LoadUTypingReplay() : base("cl_load_utyping_replay") {}
//
//     public override ConsoleResult Run(string[] consoleInput) {
//         ScreenManager.ChangeScreen(new PlayerScreen(Score.LoadUTypingReplay(consoleInput[0])));
//
//         return new ConsoleResult(ExecutionResult.Success, "Loaded UTyping replay!");
//     }
// }
//
// public class LoadAutoReplay : ConFunc {
//     public LoadAutoReplay() : base("cl_load_auto_replay") {}
//
//     public override ConsoleResult Run(string[] consoleInput) {
//         ScreenManager.ChangeScreen(new PlayerScreen(AutoReplayCreator.CreateReplay(pTypingGame.CurrentSong.Value)));
//
//         return new ConsoleResult(ExecutionResult.Success, "Loaded auto replay!");
//     }
// }
//
// public class Logout : ConFunc {
//     public Logout() : base("sv_logout") {}
//
//     public override ConsoleResult Run(string[] consoleInput) {
//         pTypingGame.OnlineManager.Logout();
//
//         if (pTypingGame.OnlineManager.State != ConnectionState.Disconnected)
//             return new(ExecutionResult.Error, "Logout not successful!");
//
//         return new(ExecutionResult.Success, "Logged out!");
//     }
// }
//
// public class Login : ConFunc {
//
//     public Login() : base("sv_login") {}
//
//     public override ConsoleResult Run(string[] consoleInput) {
//         if (consoleInput.Length == 0)
//             return new(ExecutionResult.Error, "You need to provide a username");
//         
//         ConVars.Username.Value = consoleInput[0];
//
//         pTypingGame.OnlineManager.Login();
//
//         return new(ExecutionResult.Success, "Logging in!");
//     }
// }
//
// public class SendMessage : ConFunc {
//     public SendMessage() : base("sv_send_message") {}
//     public override ConsoleResult Run(string[] consoleInput) {
//         if (pTypingGame.OnlineManager.State != ConnectionState.LoggedIn)
//             return new(ExecutionResult.Warning, "You are not logged in!");
//
//         pTypingGame.OnlineManager.SendMessage(consoleInput[0], consoleInput[1]);
//
//         return new(ExecutionResult.Success, "Message sent!");
//     }
// }
