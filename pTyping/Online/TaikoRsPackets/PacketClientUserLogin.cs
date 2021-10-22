using System.IO;

namespace pTyping.Online.TaikoRsPackets {
    public class PacketClientUserLogin : TaikoRsPacket {
        public PacketClientUserLogin(string username, string password) {
            this.Username = username;
            this.Password = password;

            this.PacketId = TaikoRsPacketId.ClientUserLogin;
        }

        public string Username;
        public string Password;
        
        protected override byte[] GetData() {
            MemoryStream  stream = new();
            TaikoRsWriter writer = new(stream);

            writer.Write(this.Username);
            writer.Write(this.Password);
            
            writer.Flush();
            
            return stream.ToArray();
        }
        
        protected override void ReadData(TaikoRsReader reader) { }
    }
}
