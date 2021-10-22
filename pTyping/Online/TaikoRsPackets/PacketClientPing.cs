using System;

namespace pTyping.Online.TaikoRsPackets {
    public class PacketClientPing : TaikoRsPacket {
        public PacketClientPing() => this.PacketId = TaikoRsPacketId.ClientPing;

        protected override byte[] GetData() => Array.Empty<byte>();
        protected override void ReadData(TaikoRsReader reader) {
            throw new NotImplementedException();
        }
    }
}
