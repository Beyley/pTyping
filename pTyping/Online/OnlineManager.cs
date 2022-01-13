using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using pTyping.Scores;
using Console=Furball.Engine.Engine.DevConsole.DevConsole;

namespace pTyping.Online {
    public abstract class OnlineManager {
        public ObservableCollection<ChatMessage>                  ChatLog       = new();
        public ObservableConcurrentDictionary<uint, OnlinePlayer> OnlinePlayers = new();

        public OnlinePlayer Player = new();

        public ConnectionState State {
            get;
            protected set;
        } = ConnectionState.Disconnected;

        public async Task SubmitScore(PlayerScore score) {
            if (this.State == ConnectionState.LoggedIn)
                await this.ClientSubmitScore(score);
        }

        [Pure]
        public async Task<List<PlayerScore>> GetMapScores(string hash) {
            if (this.State == ConnectionState.LoggedIn)
                return await this.ClientGetScores(hash);

            return new();
        }

        public abstract string Username();
        public abstract string Password();

        protected abstract void ClientLogin();
        protected abstract void ClientLogout();
        protected abstract void Connect();
        protected abstract void Disconnect();
        public abstract    void SendMessage(string channel, string message);

        protected abstract Task ClientSubmitScore(PlayerScore score);
        [Pure]
        protected abstract Task<List<PlayerScore>> ClientGetScores(string        hash);

        public abstract void ChangeUserAction(UserAction action);

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

        public void Login() {
            foreach (KeyValuePair<uint, OnlinePlayer> keyValuePair in this.OnlinePlayers)
                this.OnlinePlayers.Remove(keyValuePair.Key);

            new Thread(
            () => {
                if (this.State == ConnectionState.Disconnected)
                    this.Connect();

                if (this.State != ConnectionState.Disconnected)
                    this.ClientLogin();
            }
            ).Start();
        }

        public void Logout() {
            foreach (KeyValuePair<uint, OnlinePlayer> keyValuePair in this.OnlinePlayers)
                this.OnlinePlayers.Remove(keyValuePair.Key);

            this.ClientLogout();

            this.Disconnect();
        }

        public virtual void Initialize() {}

        public virtual void Update(GameTime time) {}
    }

    public enum ConnectionState {
        LoggedIn,
        LoggingIn,
        Connected,
        Disconnected
    }
}
