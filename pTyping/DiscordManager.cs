using System;
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Furball.Engine;
using pTyping.Graphics;

namespace pTyping;

public static class DiscordManager {
    public static DiscordRpcClient Client;

    public const long CLIENT_ID = 908631391934222366;

    public static User User { get; private set; }

    public static bool Initialized { get; private set; }
    
    public static void Initialize() {
        try {
            Initialized = true;

            int id = -1;
            if (Environment.GetEnvironmentVariable("DISCORD_INSTANCE_ID") != null)
                id = Convert.ToInt32(Environment.GetEnvironmentVariable("DISCORD_INSTANCE_ID"));

            Client = new DiscordRpcClient(CLIENT_ID.ToString(), id, new NullLogger(), false);

            Client.OnReady += OnReady;

            Client.Initialize();
        }
        catch {
            Initialized = false;
        }
    }

    private static void OnReady(object sender, ReadyMessage args) {
        User = Client.CurrentUser;
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
            Client.Invoke();

            _Timer += deltaTime;

            if (_Timer > 0.150) {// every 150ms
                UpdatePresence();
                _Timer = 0;
            }

            // LobbyManager.FlushNetwork();
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
                RichPresence presence = new() {
                    State   = screen.State,
                    Details = screen.Details,
                    Assets = new Assets {
                        LargeImageKey  = "ptyping-mode-icon",
                        LargeImageText = "pTyping"
                    }
                };

                Client.SetPresence(presence);
            }
        }
        catch {
            Initialized = false;
        }
    }
}
