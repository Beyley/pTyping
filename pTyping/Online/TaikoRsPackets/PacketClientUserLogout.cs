using System;

namespace pTyping.Online.TaikoRsPackets {
    public class PacketClientUserLogout : TaikoRsPacket {
        public PacketClientUserLogout() {
            this.PacketId = TaikoRsPacketId.Client_LogOut;
        }
        
        protected override byte[] GetData() {
            return Array.Empty<byte>();
        }
        
        protected override void ReadData(TaikoRsReader reader) {
            throw new System.NotImplementedException();
        }
    }
}
