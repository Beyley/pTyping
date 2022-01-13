using System;

namespace pTyping.Online.Taiko_rs.Packets {
    public class PacketServerUserJoined : TaikoRsPacket {
        public OnlinePlayer Player = new();

        protected override byte[] GetData() => throw new NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.Player.UserId.Value   = reader.ReadUInt32();
            this.Player.Username.Value = reader.ReadString();
            this.Player.Bot.Value      = this.Player.UserId.Value == uint.MaxValue;
        }
    }
}
