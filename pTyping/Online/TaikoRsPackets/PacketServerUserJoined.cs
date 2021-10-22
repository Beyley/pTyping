using System;

namespace pTyping.Online.TaikoRsPackets {
    public class PacketServerUserJoined : TaikoRsPacket {
        public OnlinePlayer Player = new();

        protected override byte[] GetData() => throw new NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.Player.UserId.Value   = reader.ReadInt32();
            this.Player.Username.Value = reader.ReadString();
        }
    }
}
