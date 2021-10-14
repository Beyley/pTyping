using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pTyping.Player;
using Console=Furball.Engine.Engine.Console.Console;

namespace pTyping.Online {
    public abstract class OnlineManager {
        public List<ChatMessage>             ChatLog;
        public Dictionary<int, OnlinePlayer> OnlinePlayers = new();

        public ConnectionState State {
            get;
            protected set;
        } = ConnectionState.Disconnected;

        public int UserId {
            get;
            protected set;
        }

        public UserAction UserAction {
            get;
            protected set;
        }

        public async Task SubmitScore(PlayerScore score) {
            if(this.State == ConnectionState.LoggedIn)
                await this.ClientSubmitScore(score);
        }

        public async Task<List<PlayerScore>> GetMapScores(string hash) {
            if (this.State == ConnectionState.LoggedIn)
                return await this.ClientGetScores(hash);

            return new();
        }
        
        public abstract string Username();
        public abstract string Password();

        protected abstract Task ClientLogin();
        protected abstract Task ClientLogout();
        protected abstract Task Connect();
        protected abstract Task Disconnect();
        public abstract    Task SendMessage(string            channel, string message);
        protected abstract Task ClientSubmitScore(PlayerScore score);
        protected abstract Task<List<PlayerScore>> ClientGetScores(string hash);

        public abstract Task ChangeUserAction(UserAction action);

        protected void            InvokeOnLoginStart(object sender) => this.OnLoginStart?.Invoke(sender, null);
        public event EventHandler OnLoginStart;
        protected void            InvokeOnLoginComplete(object sender) => this.OnLoginComplete?.Invoke(sender, null);
        public event EventHandler OnLoginComplete;
        protected void            InvokeOnLogout(object sender) => this.OnLogout?.Invoke(sender, null);
        public event EventHandler OnLogout;

        protected void            InvokeOnConnect(object sender) => this.OnConnect?.Invoke(sender, null);
        public event EventHandler OnConnect;
        protected void            InvokeOnConnectStart(object sender) => this.OnConnectStart?.Invoke(sender, null);
        public event EventHandler OnConnectStart;
        protected void            InvokeOnDisconnect(object sender) => this.OnDisconnect?.Invoke(sender, null);
        public event EventHandler OnDisconnect;

        public async Task Login() {
            if (this.State == ConnectionState.Disconnected)
                await this.Connect();

            if (this.State != ConnectionState.Disconnected)
                await this.ClientLogin();
        }

        public async Task Logout() {
            await this.ClientLogout();

            await this.Disconnect();
        }

        public void Initialize() {
            Console.AddFunction(
            "send_message",
            message => {
                if (this.State != ConnectionState.LoggedIn)
                    return "You are not logged in!";

                string[] split = message.Split(" ");

                this.SendMessage(split[0], string.Join(" ", split, 1, split.Length - 1)).Wait();

                return "Message sent!";
            }
            );

            Console.AddFunction(
            "login",
            _ => {
                pTypingGame.OnlineManager.Login().Wait();

                if (pTypingGame.OnlineManager.State != ConnectionState.LoggedIn)
                    return "Login not successful!";

                return "Logged in!";
            }
            );

            Console.AddFunction(
            "logout",
            _ => {
                pTypingGame.OnlineManager.Logout().Wait();

                if (pTypingGame.OnlineManager.State != ConnectionState.Disconnected)
                    return "Logout not successful!";

                return "Logged out!";
            }
            );
        }
    }

    public enum ConnectionState {
        LoggedIn,
        LoggingIn,
        Connected,
        Disconnected
    }
}
