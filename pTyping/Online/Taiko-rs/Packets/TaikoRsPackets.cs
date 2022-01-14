using System;
using System.Collections.Generic;

namespace pTyping.Online.Taiko_rs.Packets {
    // public abstract class TaikoRsPacket {
    //     public TaikoRsPacketId PacketId;
    //     public void            ReadPacket(TaikoRsReader reader) => this.ReadData(reader);
    //
    //     protected abstract byte[] GetData();
    //     protected abstract void   ReadData(TaikoRsReader reader);
    //
    //     public byte[] GetPacket() {
    //         MemoryStream  stream = new();
    //         TaikoRsWriter writer = new(stream);
    //
    //         writer.Write((ushort)this.PacketId);
    //         writer.Write(this.GetData());
    //         writer.Flush();
    //
    //         return stream.ToArray();
    //     }
    // }

    public enum DataType {
        Byte,
        SByte,
        UShort,
        Short,
        UInt,
        Int,
        ULong,
        Long,
        Single,
        Double,
        String
    }

    public abstract class Packet {
        protected Dictionary<string, object> Data = new();

        public virtual PacketId Pid => PacketId.Unknown;

        protected static readonly List<(string name, DataType type)> BLANK_DATA_DEFINITION = new();
        public virtual            List<(string name, DataType type)> DataDefinition => BLANK_DATA_DEFINITION;

        public void ReadDataFromStream(TaikoRsReader reader) {
            foreach ((string name, DataType type) in this.DataDefinition)
                switch (type) {
                    case DataType.Byte:
                        this.Data.Add(name, reader.ReadByte());
                        break;
                    case DataType.SByte:
                        this.Data.Add(name, reader.ReadSByte());
                        break;
                    case DataType.UShort:
                        this.Data.Add(name, reader.ReadUInt16());
                        break;
                    case DataType.Short:
                        this.Data.Add(name, reader.ReadInt16());
                        break;
                    case DataType.UInt:
                        this.Data.Add(name, reader.ReadUInt32());
                        break;
                    case DataType.Int:
                        this.Data.Add(name, reader.ReadInt32());
                        break;
                    case DataType.ULong:
                        this.Data.Add(name, reader.ReadUInt64());
                        break;
                    case DataType.Long:
                        this.Data.Add(name, reader.ReadInt64());
                        break;
                    case DataType.Single:
                        this.Data.Add(name, reader.ReadSingle());
                        break;
                    case DataType.Double:
                        this.Data.Add(name, reader.ReadDouble());
                        break;
                    case DataType.String:
                        this.Data.Add(name, reader.ReadString());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }
        public void WriteDataToStream(TaikoRsWriter writer) {
            #region Header

            writer.Write((ushort)this.Pid);

            #endregion

            #region Data

            foreach ((string name, DataType type) in this.DataDefinition)
                switch (type) {
                    case DataType.Byte:
                        writer.Write((byte)this.Data[name]);
                        break;
                    case DataType.SByte:
                        writer.Write((sbyte)this.Data[name]);
                        break;
                    case DataType.UShort:
                        writer.Write((ushort)this.Data[name]);
                        break;
                    case DataType.Short:
                        writer.Write((short)this.Data[name]);
                        break;
                    case DataType.UInt:
                        writer.Write((uint)this.Data[name]);
                        break;
                    case DataType.Int:
                        writer.Write((int)this.Data[name]);
                        break;
                    case DataType.ULong:
                        writer.Write((ulong)this.Data[name]);
                        break;
                    case DataType.Long:
                        writer.Write((long)this.Data[name]);
                        break;
                    case DataType.Single:
                        writer.Write((float)this.Data[name]);
                        break;
                    case DataType.Double:
                        writer.Write((double)this.Data[name]);
                        break;
                    case DataType.String:
                        writer.Write((string)this.Data[name]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            #endregion
        }
    }

    public class PingPacket : Packet {
        public override PacketId Pid => PacketId.Ping;
    }

    public class PongPacket : Packet {
        public override PacketId Pid => PacketId.Pong;
    }

    public class ClientUserLoginPacket : Packet {
        public ClientUserLoginPacket(string username, string password) {
            this.Username        = username;
            this.Password        = password;
            this.ProtocolVersion = TaikoRsOnlineManager.PROTOCOL_VERSION;
            this.Game            = $"pTyping\n{Program.BuildVersion}";
        }
        public override PacketId Pid => PacketId.ClientUserLogin;

        private static readonly List<(string name, DataType type)> DATA_DEFINITION = new() {
            ("protocol_version", DataType.UShort),
            ("username", DataType.String),
            ("password", DataType.String),
            ("game", DataType.String)
        };

        public ushort ProtocolVersion {
            get => (ushort)this.Data["protocol_version"];
            set => this.Data["protocol_version"] = value;
        }
        public string Username {
            get => (string)this.Data["username"];
            set => this.Data["username"] = value;
        }
        public string Password {
            get => (string)this.Data["password"];
            set => this.Data["password"] = value;
        }
        public string Game {
            get => (string)this.Data["game"];
            set => this.Data["game"] = value;
        }

        public override List<(string name, DataType type)> DataDefinition => DATA_DEFINITION;
    }

    public enum LoginStatus : byte {
        UnknownError = 0,
        Ok           = 1,
        BadPassword  = 2,
        NoUser       = 3
    }

    public class ServerLoginResponsePacket : Packet {
        public override PacketId Pid => PacketId.ServerLoginResponse;

        private static readonly List<(string name, DataType type)> DATA_DEFINITION = new() {
            ("status", DataType.Byte),
            ("user_id", DataType.UInt)
        };

        public LoginStatus LoginStatus {
            get => (LoginStatus)this.Data["status"];
            set => this.Data["status"] = value;
        }
        public uint UserId {
            get => (uint)this.Data["user_id"];
            set => this.Data["user_id"] = value;
        }

        public override List<(string name, DataType type)> DataDefinition => DATA_DEFINITION;
    }

    public class ServerPermissionsPacket : Packet {
        public override PacketId Pid => PacketId.ServerPermissions;

        private static readonly List<(string name, DataType type)> DATA_DEFINITION = new() {
            ("user_id", DataType.UInt),
            ("permissions", DataType.UShort)
        };

        public ServerPermissions Permissions {
            get => (ServerPermissions)this.Data["permissions"];
            set => this.Data["permissions"] = value;
        }
        public uint UserId {
            get => (uint)this.Data["user_id"];
            set => this.Data["user_id"] = value;
        }

        public override List<(string name, DataType type)> DataDefinition => DATA_DEFINITION;
    }

    public class ServerUserJoinedPacket : Packet {
        public override PacketId Pid => PacketId.ServerPermissions;

        private static readonly List<(string name, DataType type)> DATA_DEFINITION = new() {
            ("user_id", DataType.UInt),
            ("username", DataType.String)
        };

        public string Username {
            get => (string)this.Data["username"];
            set => this.Data["username"] = value;
        }
        public uint UserId {
            get => (uint)this.Data["user_id"];
            set => this.Data["user_id"] = value;
        }

        public override List<(string name, DataType type)> DataDefinition => DATA_DEFINITION;
    }

    public class ServerUserLeftPacket : Packet {
        public override PacketId Pid => PacketId.ServerUserLeft;

        private static readonly List<(string name, DataType type)> DATA_DEFINITION = new() {
            ("user_id", DataType.UInt)
        };

        public uint UserId {
            get => (uint)this.Data["user_id"];
            set => this.Data["user_id"] = value;
        }

        public override List<(string name, DataType type)> DataDefinition => DATA_DEFINITION;
    }

    public class ClientStatusUpdatePacket : Packet {
        public ClientStatusUpdatePacket(UserAction action) => this.Action = action;
        public override PacketId Pid => PacketId.ClientStatusUpdate;

        private static readonly List<(string name, DataType type)> DATA_DEFINITION = new() {
            ("action", DataType.UShort),
            ("action_text", DataType.String),
            ("mode", DataType.Byte)
        };

        public UserAction Action {
            get => new((UserActionType)this.Data["action"], this.Data["action_text"].ToString(), (PlayMode)this.Data["mode"]);
            set {
                this.Data["action"]      = value.Action.Value;
                this.Data["action_text"] = value.ActionText.Value;
                this.Data["mode"]        = value.Mode.Value;
            }
        }

        public override List<(string name, DataType type)> DataDefinition => DATA_DEFINITION;
    }

    public class ServerUserStatusUpdatePacket : Packet {
        public override PacketId Pid => PacketId.ServerUserStatusUpdate;

        private static readonly List<(string name, DataType type)> DATA_DEFINITION = new() {
            ("user_id", DataType.UInt),
            ("action", DataType.UShort),
            ("action_text", DataType.String),
            ("mode", DataType.Byte)
        };

        public uint UserId {
            get => (uint)this.Data["user_id"];
            set => this.Data["user_id"] = value;
        }

        public UserAction Action {
            get => new((UserActionType)this.Data["action"], this.Data["action_text"].ToString(), (PlayMode)this.Data["mode"]);
            set {
                this.Data["action"]      = value.Action.Value;
                this.Data["action_text"] = value.ActionText.Value;
                this.Data["mode"]        = value.Mode.Value;
            }
        }

        public override List<(string name, DataType type)> DataDefinition => DATA_DEFINITION;
    }

    public class ClientNotifyScoreUpdatePacket : Packet {
        public override PacketId Pid => PacketId.ClientNotifyScoreUpdate;
    }

    public class ServerScoreUpdatePacket : Packet {
        public override PacketId Pid => PacketId.ServerScoreUpdate;

        private static readonly List<(string name, DataType type)> DATA_DEFINITION = new() {
            ("user_id", DataType.UInt),
            ("total_score", DataType.Long),
            ("ranked_score", DataType.Long),
            ("accuracy", DataType.Double),
            ("play_count", DataType.Int),
            ("rank", DataType.Int)
        };

        public uint UserId {
            get => (uint)this.Data["user_id"];
            set => this.Data["user_id"] = value;
        }

        public long TotalScore {
            get => (long)this.Data["total_score"];
            set => this.Data["total_score"] = value;
        }
        public long RankedScore {
            get => (long)this.Data["ranked_score"];
            set => this.Data["ranked_score"] = value;
        }
        public double Accuracy {
            get => (double)this.Data["accuracy"];
            set => this.Data["accuracy"] = value;
        }
        public int Playcount {
            get => (int)this.Data["play_count"];
            set => this.Data["play_count"] = value;
        }
        public int Rank {
            get => (int)this.Data["rank"];
            set => this.Data["rank"] = value;
        }

        public override List<(string name, DataType type)> DataDefinition => DATA_DEFINITION;
    }

    public class ClientSendMessagePacket : Packet {
        public ClientSendMessagePacket(string channel, string message) {
            this.Channel = channel;
            this.Message = message;
        }
        public override PacketId Pid => PacketId.ClientSendMessage;

        private static readonly List<(string name, DataType type)> DATA_DEFINITION = new() {
            ("channel", DataType.String),
            ("message", DataType.String)
        };

        public string Channel {
            get => (string)this.Data["channel"];
            set => this.Data["channel"] = value;
        }
        public string Message {
            get => (string)this.Data["message"];
            set => this.Data["message"] = value;
        }

        public override List<(string name, DataType type)> DataDefinition => DATA_DEFINITION;
    }

    public class ServerSendMessagePacket : Packet {
        public override PacketId Pid => PacketId.ServerSendMessage;

        private static readonly List<(string name, DataType type)> DATA_DEFINITION = new() {
            ("sender_id", DataType.UInt),
            ("channel", DataType.String),
            ("message", DataType.String)
        };

        public uint SenderId {
            get => (uint)this.Data["sender_id"];
            set => this.Data["sender_id"] = value;
        }
        public string Channel {
            get => (string)this.Data["channel"];
            set => this.Data["channel"] = value;
        }
        public string Message {
            get => (string)this.Data["message"];
            set => this.Data["message"] = value;
        }

        public override List<(string name, DataType type)> DataDefinition => DATA_DEFINITION;
    }

    public class ClientLeaveSpectatorPacket : Packet {
        public override PacketId Pid => PacketId.ClientLeaveSpectator;
    }

    public class ClientLogOutPacket : Packet {
        public override PacketId Pid => PacketId.ClientLogOut;
    }

    public enum PacketId : ushort {
        /// <summary>
        ///     We dont know what packet this is
        /// </summary>
        Unknown = 0,

        #region Ping

        Ping = 1,
        Pong = 2,

        #endregion

        #region Login

        ClientUserLogin     = 100,
        ServerLoginResponse = 101,
        ServerPermissions   = 102,
        ServerUserJoined    = 103,
        ClientLogOut        = 104,
        ServerUserLeft      = 105,

        #endregion

        #region Status Updates

        ClientStatusUpdate      = 200,
        ServerUserStatusUpdate  = 201,
        ClientNotifyScoreUpdate = 202,
        ServerScoreUpdate       = 203,

        #endregion

        #region Chat

        ClientSendMessage = 300,
        ServerSendMessage = 301,

        #endregion

        #region Spectator

        ClientSpectate                = 400,
        ServerSpectatorJoined         = 401,
        ClientLeaveSpectator          = 402,
        ServerSpectatorLeft           = 403,
        ClientSpectatorFrames         = 404,
        ServerSpectatorFrames         = 405,
        ServerSpectatorPlayingRequest = 406

        #endregion
    }
}
