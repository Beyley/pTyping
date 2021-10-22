using System;

namespace pTyping.Online.TaikoRsPackets {
    public class PacketServerUserLeft : TaikoRsPacket {
        public int UserId;

        protected override byte[] GetData() => throw new NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.UserId = reader.ReadInt32();
        }
    }
}
