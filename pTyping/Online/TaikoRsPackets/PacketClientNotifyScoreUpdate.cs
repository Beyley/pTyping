using System;

namespace pTyping.Online.TaikoRsPackets {
    public class PacketClientNotifyScoreUpdate : TaikoRsPacket {
        public PacketClientNotifyScoreUpdate() {
            this.PacketId = TaikoRsPacketId.Client_NotifyScoreUpdate;
        }
        
        protected override byte[] GetData() => Array.Empty<byte>();

        protected override void ReadData(TaikoRsReader reader) => throw new NotImplementedException();
    }
}
