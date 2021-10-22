using System;

namespace pTyping.Online.TaikoRsPackets {
    public class PacketServerScoreUpdate : TaikoRsPacket {
        public double Accuracy;
        public int    PlayCount;
        public long   RankedScore;
        public long   TotalScore;
        public int    UserId;

        protected override byte[] GetData() => throw new NotImplementedException();

        protected override void ReadData(TaikoRsReader reader) {
            this.UserId      = reader.ReadInt32();
            this.TotalScore  = reader.ReadInt64();
            this.RankedScore = reader.ReadInt64();
            this.Accuracy    = reader.ReadDouble();
            this.PlayCount   = reader.ReadInt32();
        }
    }
}
