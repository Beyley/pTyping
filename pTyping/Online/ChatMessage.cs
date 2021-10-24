using System;

namespace pTyping.Online {
    public class ChatMessage {
        public string       Channel;
        public string       Message;
        public OnlinePlayer Sender;
        public DateTime     Time;

        public ChatMessage(OnlinePlayer sender, string channel, string message) {
            this.Sender  = sender;
            this.Channel = channel;
            this.Message = message;
            this.Time    = DateTime.Now;
        }

        public override string ToString() => $"<{this.Time.Hour:00}:{this.Time.Minute:00}> {this.Sender}: {this.Message}";
    }
}
