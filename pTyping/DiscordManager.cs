using DiscordSDK;
using Furball.Engine;
using Furball.Engine.Engine.Helpers;
using pTyping.Graphics;
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
    
    public static void Initialize() {
        Client             = new(CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);
        ActivityManager    = Client.GetActivityManager();
        ApplicationManager = Client.GetApplicationManager();
        LobbyManager       = Client.GetLobbyManager();
        UserManager        = Client.GetUserManager();

        UserManager.OnCurrentUserUpdate += OnUserUpdate;

        ActivityManager.OnActivityJoin += OnActivityJoin;
    }
    private static void OnActivityJoin(string secret) {
        LobbyManager.ConnectLobbyWithActivitySecret(
        secret,
        (Result result, ref Lobby lobby) => {
            Lobby.Value = lobby;

            DiscordOnlineLobby online = new(Lobby);
        }
        );
    }

    public static void CreateLobby() {
        LobbyTransaction transaction = LobbyManager.GetLobbyCreateTransaction();
        transaction.SetType(LobbyType.Private);
        transaction.SetCapacity(8);

        LobbyManager.CreateLobby(
        transaction,
        (Result result, ref Lobby lobby) => {
            if (result != Result.Ok)
                return;

            Lobby.Value = lobby;

            UpdatePresence();

            DiscordOnlineLobby online = new(Lobby);
        }
        );
    }

    public static void UpdateLobbyObject() {
        if (Lobby.Value.HasValue)
            Lobby.Value = LobbyManager.GetLobby(Lobby.Value.Value.Id);
    }

    private static void OnUserUpdate() {
        User = UserManager.GetCurrentUser();
    }
    
    public static void Dispose() {
        Client.Dispose();
    }

    private static double _Timer = 0;
    public static void Update(double deltaTime) {
        Client.RunCallbacks();

        _Timer += deltaTime;

        if (_Timer > 1) {
            UpdatePresence();
            _Timer = 0;
        }

        LobbyManager.FlushNetwork();
    }

    private static void UpdatePresence() {
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
}
