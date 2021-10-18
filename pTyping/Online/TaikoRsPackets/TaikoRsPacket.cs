using System.IO;

namespace pTyping.Online.TaikoRsPackets {
    public abstract class TaikoRsPacket {
        public TaikoRsPacket() { }

        public void ReadPacket(TaikoRsReader reader) => this.ReadData(reader);

        public TaikoRsPacketId PacketId;

        protected abstract byte[] GetData();
        protected abstract void ReadData(TaikoRsReader reader);

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
        Client_UserLogin,
        Server_LoginResponse,

        // status updates
        Client_StatusUpdate,
        Server_UserStatusUpdate,
        Client_LogOut,
        Server_UserJoined,
        Server_UserLeft,

        // chat
        Client_SendMessage,// sender_id, channel_id, message
        Server_SendMessage,// sender_id, channel_id, message

        // spectator?
        Client_Spectate,       // user_id to spectate
        Server_SpectatorJoined,// user_id of spectator
        Client_SpectatorFrames,// frame_count, [SpectatorFrame]
        Server_SpectatorFrames,// sender_id, frame_count, [SpectatorFrame]

        // multiplayer?
    }
}
