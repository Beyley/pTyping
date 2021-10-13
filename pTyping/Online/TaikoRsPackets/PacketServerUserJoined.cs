namespace pTyping.Online.TaikoRsPackets {
    public class PacketServerUserJoined : TaikoRsPacket {
        public OnlinePlayer Player = new();
        
        protected override byte[] GetData() => throw new System.NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.Player.UserId   = reader.ReadInt32();
            this.Player.Username = reader.ReadString();
        }
    }
}
