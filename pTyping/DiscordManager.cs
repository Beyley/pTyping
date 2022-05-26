using DiscordSDK;
using Furball.Engine;
using pTyping.Graphics;

namespace pTyping;

public static class DiscordManager {
    public static Discord            DiscordClient;
    public static ActivityManager    DiscordActivityManager;
    public static ApplicationManager DiscordApplicationManager;

    public const long CLIENT_ID = 908631391934222366;

    public static void Initialize() {
        DiscordClient             = new(CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);
        DiscordActivityManager    = DiscordClient.GetActivityManager();
        DiscordApplicationManager = DiscordClient.GetApplicationManager();
    }
    public static void Dispose() {
        DiscordClient.Dispose();
    }

    private static double _Timer = 0;
    public static void Update(double deltaTime) {
        DiscordClient.RunCallbacks();

        _Timer += deltaTime;

        if (_Timer > 1) {
            UpdatePresence();
            _Timer = 0;
        }
    }

    private static void UpdatePresence() {
        if (FurballGame.Instance.RunningScreen is pScreen screen) {
            Activity activity = new() {
                State   = screen.State,
                Details = screen.Details
            };

            DiscordActivityManager.UpdateActivity(activity, _ => {});
        }
    }
}
