using System;

namespace pTyping.Online {
    public class ChatMessage {
        public OnlinePlayer Sender;
        public string       Channel;
        public string       Message;
        public DateTime     Time;

        public ChatMessage(OnlinePlayer sender, string channel, string message) {
            this.Sender  = sender;
            this.Channel = channel;
            this.Message = message;
            this.Time    = DateTime.Now;
        }
    }
}
