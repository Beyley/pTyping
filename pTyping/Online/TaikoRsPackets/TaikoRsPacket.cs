using System.IO;

namespace pTyping.Online.TaikoRsPackets {
    public abstract class TaikoRsPacket {

        public TaikoRsPacketId PacketId;
        public void            ReadPacket(TaikoRsReader reader) => this.ReadData(reader);

        protected abstract byte[] GetData();
        protected abstract void   ReadData(TaikoRsReader reader);

        public byte[] GetPacket() {
            MemoryStream  stream = new();
            TaikoRsWriter writer = new(stream);

            writer.Write((ushort)this.PacketId);
            writer.Write(this.GetData());
            writer.Flush();

            return stream.ToArray();
        }
    }

    public enum TaikoRsPacketId : ushort {
        Unknown = 0,

        // login
        ClientUserLogin,
        ServerLoginResponse,

        // status updates
        ClientStatusUpdate,
        ServerUserStatusUpdate,
        ClientNotifyScoreUpdate,
        ServerScoreUpdate,
        ClientLogOut,
        ServerUserJoined,
        ServerUserLeft,
        ClientPing,
        ServerPong,

        // chat
        ClientSendMessage,// sender_id, channel_id, message
        ServerSendMessage,// sender_id, channel_id, message

        // spectator?
        ClientSpectate,       // user_id to spectate
        ServerSpectatorJoined,// user_id of spectator
        ClientSpectatorFrames,// frame_count, [SpectatorFrame]
        ServerSpectatorFrames // sender_id, frame_count, [SpectatorFrame]

        // multiplayer?
    }
}
