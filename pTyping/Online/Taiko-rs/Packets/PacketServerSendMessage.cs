using System;

namespace pTyping.Online.Taiko_rs.Packets {
    public class PacketServerSendMessage : TaikoRsPacket {
        public string Channel;
        public string Message;
        public uint   UserId;

        protected override byte[] GetData() => throw new NotImplementedException();
        protected override void ReadData(TaikoRsReader reader) {
            this.UserId  = reader.ReadUInt32();
            this.Message = reader.ReadString();
            this.Channel = reader.ReadString();
        }
    }
}
