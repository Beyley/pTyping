namespace pTyping.Online.TaikoRsPackets {
    public class PacketServerSendMessage : TaikoRsPacket {
        public int    UserId;
        public string Message;
        public string Channel;
        
        protected override byte[] GetData() => throw new System.NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.UserId  = reader.ReadInt32();
            this.Message = reader.ReadString();
            this.Channel = reader.ReadString();
        }
    }
}
