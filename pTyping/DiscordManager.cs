using DiscordSDK;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Helpers;
using pTyping.Engine;
using pTyping.Graphics;
using pTyping.Graphics.Menus.Online;
using pTyping.Online.Discord;

namespace pTyping;

public static class DiscordManager {
    public static Discord            Client;
    public static ActivityManager    ActivityManager;
    public static ApplicationManager ApplicationManager;
    public static LobbyManager       LobbyManager;
    public static UserManager        UserManager;

    public const long CLIENT_ID = 908631391934222366;

    public static User User { get; private set; }

    public static Bindable<Lobby?> Lobby {
        get;
    } = new(null);
    
    public static bool Initialized { get; private set; }
    
    public static void Initialize() {
        try {
            Initialized = true;

            Client             = new(CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);

            ActivityManager    = Client.GetActivityManager();
            ApplicationManager = Client.GetApplicationManager();
            LobbyManager       = Client.GetLobbyManager();
            UserManager        = Client.GetUserManager();

            UserManager.OnCurrentUserUpdate += OnUserUpdate;

            ActivityManager.OnActivityJoin += OnActivityJoin;
        }
        catch {
            Initialized = false;
        }
    }
    private static void OnActivityJoin(string secret) {
        LobbyManager.ConnectLobbyWithActivitySecret(
        secret,
        (Result result, ref Lobby lobby) => {
            if (result != Result.Ok) {
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "Unable to join lobby!");

                return;
            }

            Lobby _lobby = lobby;

            FurballGame.GameTimeScheduler.ScheduleMethod(
            _ => {
                Lobby.Value = _lobby;

                pTypingGame.OnlineManager.OnlineLobby = new DiscordOnlineLobby(Lobby);

                ScreenManager.ChangeScreen(new LobbyScreen());
            },
            0
            );
        }
        );
    }

    public static bool CreateLobby() {
        if (!Initialized)
            return false;

        try {
            LobbyTransaction transaction = LobbyManager.GetLobbyCreateTransaction();
            transaction.SetType(LobbyType.Public);
            transaction.SetCapacity(8);
            transaction.SetMetadata("name", $"{User.Username}'s game");

            LobbyManager.CreateLobby(
            transaction,
            (Result result, ref Lobby lobby) => {
                if (result != Result.Ok)
                    return;

                Lobby.Value = lobby;

                UpdatePresence();

                pTypingGame.OnlineManager.OnlineLobby = new DiscordOnlineLobby(Lobby);

                ScreenManager.ChangeScreen(new LobbyScreen());
            }
            );
        }
        catch {
            Initialized = false;
            return false;
        }

        return true;
    }

    public static bool UpdateLobbyObject() {
        if (!Initialized)
            return false;
        
        if (Lobby.Value.HasValue)
            Lobby.Value = LobbyManager.GetLobby(Lobby.Value.Value.Id);

        return true;
    }

    private static void OnUserUpdate() {
        if (!Initialized)
            return;
        
        User = UserManager.GetCurrentUser();
    }
    
    public static void Dispose() {
        if (!Initialized)
            return;
        
        Client.Dispose();
        Initialized = false;
    }

    private static double _Timer = 0;
    public static void Update(double deltaTime) {
        if (!Initialized)
            return;
        try {
            Client.RunCallbacks();

            _Timer += deltaTime;

            if (_Timer > 1) {
                UpdatePresence();
                _Timer = 0;
            }

            LobbyManager.FlushNetwork();
        }
        catch {
            Initialized = false;
        }
    }

    private static void UpdatePresence() {
        if (!Initialized)
            return;

        try {
            if (FurballGame.Instance.RunningScreen is pScreen screen) {
                Activity activity = new() {
                    State   = screen.State,
                    Details = screen.Details,
                    Assets = new ActivityAssets {
                        LargeImage = "ptyping-mode-icon",
                        LargeText  = "pTyping"
                    }
                };

                if (Lobby.Value.HasValue) {
                    Lobby lobby = Lobby.Value.Value;

                    activity.Party = new ActivityParty {
                        Id = lobby.Id.ToString(),
                        Size = {
                            CurrentSize = LobbyManager.MemberCount(lobby.Id),
                            MaxSize     = (int)lobby.Capacity
                        }
                    };
                    activity.Secrets = new ActivitySecrets {
                        Join = LobbyManager.GetLobbyActivitySecret(lobby.Id)
                    };
                }

                ActivityManager.UpdateActivity(activity, _ => {});
            }
        }
        catch {
            Initialized = false;
        }
    }
}
