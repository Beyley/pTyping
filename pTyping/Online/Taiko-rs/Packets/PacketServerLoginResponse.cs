using System;

namespace pTyping.Online.Taiko_rs.Packets {
    public class PacketServerLoginResponse : TaikoRsPacket {
        public int UserId = 0;

        protected override byte[] GetData() => throw new NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.UserId = reader.ReadInt32();
        }
    }
}
