using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Console=Furball.Engine.Engine.Console.Console;

namespace pTyping.Online {
    public abstract class OnlineManager {
        public ChatManager  Chat;
        public Dictionary<int, OnlinePlayer> Players = new(); 

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

        public abstract string Username();
        public abstract string Password();

        protected abstract Task ClientLogin();
        protected abstract Task ClientLogout();
        protected abstract Task Connect();
        protected abstract Task Disconnect();
        public virtual async Task SendMessage(string channel, string message) { }
        
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

        public async void Login() {
            if(this.State == ConnectionState.Disconnected)
                await this.Connect();
            
            await this.ClientLogin();
        }

        public async Task Logout() {
            await this.ClientLogout();

            await this.Disconnect();
        }

        public void Initialize() {
            Console.AddFunction("send_message",  (message) => {
                if(this.State != ConnectionState.Connected)
                    return "You are not connected!";

                string[] split = message.Split(" ");
                
                this.SendMessage(split[0], string.Join(" ", split, 1, split.Length - 1)).Wait();
                
                return "Message sent!";
            });
        }
    }

    public enum ConnectionState {
        Connected,
        Connecting,
        Disconnected
    }
}
