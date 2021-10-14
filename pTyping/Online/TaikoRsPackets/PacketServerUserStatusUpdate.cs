namespace pTyping.Online.TaikoRsPackets {
    public class PacketServerUserStatusUpdate : TaikoRsPacket {
        public int        UserId;
        public UserAction Action = new();
        
        protected override byte[] GetData() => throw new System.NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.UserId            = reader.ReadInt32();
            this.Action.Action     = (UserActionType)reader.ReadUInt16();
            this.Action.ActionText = reader.ReadString();
            this.Action.Mode       = (PlayMode)reader.ReadByte();
        }
    }
}
