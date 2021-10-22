using System.IO;

namespace pTyping.Online.TaikoRsPackets {
    public class PacketClientSendMessage : TaikoRsPacket {
        public string Message;
        public string Channel;
        
        public PacketClientSendMessage(string channel, string message) {
            this.Message = message;
            this.Channel = channel;

            this.PacketId = TaikoRsPacketId.ClientSendMessage;
        }
        
        protected override byte[] GetData() {
            MemoryStream  stream = new();
            TaikoRsWriter writer = new(stream);
            
            writer.Write(this.Message);
            writer.Write(this.Channel);

            writer.Flush();
            writer.Close();
            
            return stream.ToArray();
        }
        protected override void ReadData(TaikoRsReader reader) {
            throw new System.NotImplementedException();
        }
    }
}
