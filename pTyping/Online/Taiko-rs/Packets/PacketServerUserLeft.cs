using System;

namespace pTyping.Online.Taiko_rs.Packets {
    public class PacketServerUserLeft : TaikoRsPacket {
        public uint UserId;

        protected override byte[] GetData() => throw new NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.UserId = reader.ReadUInt32();
        }
    }
}
