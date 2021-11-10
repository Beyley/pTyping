using System;

namespace pTyping.Online.Taiko_rs.Packets {
    public class PacketClientUserLogout : TaikoRsPacket {
        public PacketClientUserLogout() => this.PacketId = TaikoRsPacketId.ClientLogOut;

        protected override byte[] GetData() => Array.Empty<byte>();

        protected override void ReadData(TaikoRsReader reader) {
            throw new NotImplementedException();
        }
    }
}
