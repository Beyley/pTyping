using System.IO;

namespace pTyping.Online.TaikoRsPackets {
    public class PacketClientStatusUpdate : TaikoRsPacket {
        public UserAction Action;

        public PacketClientStatusUpdate(UserAction action) {
            this.Action = action;

            this.PacketId = TaikoRsPacketId.Client_StatusUpdate;
        }
        
        protected override byte[] GetData() {
            MemoryStream  stream = new();
            TaikoRsWriter writer = new(stream);
            
            writer.Write((ushort)this.Action.Action.Value);
            writer.Write(this.Action.ActionText);
            writer.Write((byte)this.Action.Mode.Value);
            
            writer.Flush();

            return stream.ToArray();
        }
        
        protected override void ReadData(TaikoRsReader reader) {
            throw new System.NotImplementedException();
        }
    }
}
