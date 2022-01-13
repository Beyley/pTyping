using System;

namespace pTyping.Online.Taiko_rs.Packets {
    public class PacketServerUserStatusUpdate : TaikoRsPacket {
        public UserAction Action = new();
        public uint       UserId;

        protected override byte[] GetData() => throw new NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.UserId                  = reader.ReadUInt32();
            this.Action.Action.Value     = (UserActionType)reader.ReadUInt16();
            this.Action.ActionText.Value = reader.ReadString();
            this.Action.Mode.Value       = (PlayMode)reader.ReadByte();
        }
    }
}
