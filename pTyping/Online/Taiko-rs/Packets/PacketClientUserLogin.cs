using System.IO;

namespace pTyping.Online.Taiko_rs.Packets {
    public class PacketClientUserLogin : TaikoRsPacket {
        public string Password;

        public string Username;
        public PacketClientUserLogin(string username, string password) {
            this.Username = username;
            this.Password = password;

            this.PacketId = TaikoRsPacketId.ClientUserLogin;
        }

        protected override byte[] GetData() {
            MemoryStream  stream = new();
            TaikoRsWriter writer = new(stream);

            writer.Write(this.Username);
            writer.Write(this.Password);

            writer.Flush();

            return stream.ToArray();
        }

        protected override void ReadData(TaikoRsReader reader) {}
    }
}
