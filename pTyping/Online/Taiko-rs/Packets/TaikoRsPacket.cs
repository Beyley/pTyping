using System.IO;

namespace pTyping.Online.Taiko_rs.Packets {
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
        ClientUserLogin     = 1,
        ServerLoginResponse = 2,

        // status updates
        ClientStatusUpdate      = 3,
        ServerUserStatusUpdate  = 4,
        ClientNotifyScoreUpdate = 5,
        ServerScoreUpdate       = 6,
        ClientLogOut            = 7,
        ServerUserJoined        = 8,
        ServerUserLeft          = 9,

        // ======= chat =======

        /// sender_id, channel_id, message
        ClientSendMessage = 10,
        /// sender_id, channel_id, message
        ServerSendMessage = 11,

        // ======= spectator =======

        /// client wants to spectate someone
        ClientSpectate = 100,       // user_id to spectate
        ServerSpectatorJoined = 101,// user_id of spectator who joined

        /// "LeftSpectator" fits better, but it doesnt line up with the server packet name
        ClientSpectatorLeft = 102,
        /// user_id of spectator who left
        /// if user_id is your own, you stopped spectating
        ServerSpectatorLeft = 103,

        ClientSpectatorFrames         = 104,// frame_count, [SpectatorFrame]
        ServerSpectatorFrames         = 105,// host_id, frame_count, [SpectatorFrame]
        ServerSpectatorPlayingRequest = 106,


        // ======= ping =======
        Ping = 200,
        Pong = 201

        // multiplayer?
    }
}
