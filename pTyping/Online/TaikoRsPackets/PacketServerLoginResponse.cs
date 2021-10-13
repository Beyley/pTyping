namespace pTyping.Online.TaikoRsPackets {
    public class PacketServerLoginResponse : TaikoRsPacket {
        public int UserId = 0;
        
        protected override byte[] GetData() => throw new System.NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.UserId = reader.ReadInt32();
        }
    }
}
