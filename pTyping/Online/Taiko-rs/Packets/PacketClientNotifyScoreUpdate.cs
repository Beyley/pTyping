using System;

namespace pTyping.Online.Taiko_rs.Packets {
    public class PacketClientNotifyScoreUpdate : TaikoRsPacket {
        public PacketClientNotifyScoreUpdate() => this.PacketId = TaikoRsPacketId.ClientNotifyScoreUpdate;

        protected override byte[] GetData() => Array.Empty<byte>();

        protected override void ReadData(TaikoRsReader reader) => throw new NotImplementedException();
    }
}
