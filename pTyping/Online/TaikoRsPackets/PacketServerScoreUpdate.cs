namespace pTyping.Online.TaikoRsPackets {
    public class PacketServerScoreUpdate : TaikoRsPacket {
        public int    UserId;
        public long   TotalScore;
        public long   RankedScore;
        public double Accuracy;
        public int    PlayCount;

        protected override byte[] GetData() => throw new System.NotImplementedException();
        
        protected override void ReadData(TaikoRsReader reader) {
            this.UserId      = reader.ReadInt32();
            this.TotalScore  = reader.ReadInt64();
            this.RankedScore = reader.ReadInt64();
            this.Accuracy    = reader.ReadDouble();
            this.PlayCount   = reader.ReadInt32();
        }
    }
}
