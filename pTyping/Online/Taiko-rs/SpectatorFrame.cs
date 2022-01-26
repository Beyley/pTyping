using System;
using Newtonsoft.Json;
using pTyping.Scores;

namespace pTyping.Online.Taiko_rs;

public enum SpectatorFrameDataType : byte {
    Play            = 0,
    Pause           = 1,
    Resume          = 2,
    Buffer          = 3,
    SpectatingOther = 4,
    ReplayFrame     = 5,
    ScoreSync       = 6,
    ChangingMap     = 7,
    PlayingResponse = 8,
    Unknown         = 255
}

[JsonObject(MemberSerialization.OptIn)]
public class SpectatorModInfo {
    [JsonProperty("speed")]
    public float Speed;
    [JsonProperty("autoplay")]
    public bool Autoplay;
}

public class SpectatorFramePlay : SpectatorFrame {
    public override SpectatorFrameDataType Type => SpectatorFrameDataType.Play;

    public string           BeatmapHash;
    public PlayMode         Mode;
    public SpectatorModInfo Modinfo;
}

public class SpectatorFramePause : SpectatorFrame {
    public override SpectatorFrameDataType Type => SpectatorFrameDataType.Pause;
}

public class SpectatorFrameUnpause : SpectatorFrame {
    public override SpectatorFrameDataType Type => SpectatorFrameDataType.Resume;
}

public class SpectatorFrameBuffer : SpectatorFrame {
    public override SpectatorFrameDataType Type => SpectatorFrameDataType.Buffer;
}

/// <summary>
///     The host started spectating someone else
/// </summary>
public class SpectatorFrameSpectatingOther : SpectatorFrame {
    public override SpectatorFrameDataType Type => SpectatorFrameDataType.SpectatingOther;

    public uint UserId;
}

public class SpectatorFrameReplayFrame : SpectatorFrame {
    public override SpectatorFrameDataType Type => SpectatorFrameDataType.ReplayFrame;

    public ReplayFrame Frame;
}

public class SpectatorFrameScoreSync : SpectatorFrame {
    public override SpectatorFrameDataType Type => SpectatorFrameDataType.ScoreSync;

    public PlayerScore Score;
}

public class SpectatorFrameChangingMap : SpectatorFrame {
    public override SpectatorFrameDataType Type => SpectatorFrameDataType.ChangingMap;
}

public class SpectatorFramePlayingResponse : SpectatorFrame {
    public override SpectatorFrameDataType Type => SpectatorFrameDataType.PlayingResponse;

    public uint             UserId;
    public string           BeatmapHash;
    public PlayMode         Mode;
    public SpectatorModInfo Mods;
    public float            CurrentTime;
}

public abstract class SpectatorFrame {
    public float Time = 0f;

    public abstract SpectatorFrameDataType Type { get; }

    public void WriteFrame(TaikoRsWriter writer) {
        writer.Write(this.Time);
        writer.Write((byte)this.Type);

        switch (this.Type) {
            case SpectatorFrameDataType.Play: {
                SpectatorFramePlay frame = (SpectatorFramePlay)this;

                writer.Write(frame.BeatmapHash);
                writer.Write((byte)frame.Mode);
                writer.Write(JsonConvert.SerializeObject(frame.Modinfo));
                break;
            }
            case SpectatorFrameDataType.SpectatingOther: {
                SpectatorFrameSpectatingOther frame = (SpectatorFrameSpectatingOther)this;

                writer.Write(frame.UserId);

                break;
            }
            case SpectatorFrameDataType.ReplayFrame: {
                SpectatorFrameReplayFrame frame = (SpectatorFrameReplayFrame)this;

                frame.Frame.TaikoRsSerialize(writer);

                break;
            }
            case SpectatorFrameDataType.ScoreSync: {
                SpectatorFrameScoreSync frame = (SpectatorFrameScoreSync)this;

                frame.Score.TaikoRsSerialize(writer);

                break;
            }
            case SpectatorFrameDataType.PlayingResponse: {
                SpectatorFramePlayingResponse frame = (SpectatorFramePlayingResponse)this;

                writer.Write(frame.UserId);
                writer.Write(frame.BeatmapHash);
                writer.Write((byte)frame.Mode);
                writer.Write(JsonConvert.SerializeObject(frame.Mods));
                writer.Write(frame.CurrentTime);

                break;
            }
        }
    }

    public static SpectatorFrame ReadFrame(TaikoRsReader reader) {
        SpectatorFrame frame;

        float time = reader.ReadSingle();

        SpectatorFrameDataType type = reader.ReadSpectatorFrameType();

        switch (type) {
            case SpectatorFrameDataType.Play:
                SpectatorFramePlay playFrame = new();

                playFrame.BeatmapHash = reader.ReadString();
                playFrame.Mode        = reader.ReadPlayMode();
                playFrame.Modinfo     = JsonConvert.DeserializeObject<SpectatorModInfo>(reader.ReadString());

                frame = playFrame;
                break;
            case SpectatorFrameDataType.Pause:
                frame = new SpectatorFramePause();
                break;
            case SpectatorFrameDataType.Resume:
                frame = new SpectatorFrameUnpause();
                break;
            case SpectatorFrameDataType.Buffer:
                frame = new SpectatorFrameBuffer();
                break;
            case SpectatorFrameDataType.SpectatingOther: {
                SpectatorFrameSpectatingOther tframe = new();

                tframe.UserId = reader.ReadUInt32();

                frame = tframe;
                break;
            }
            case SpectatorFrameDataType.ReplayFrame: {
                SpectatorFrameReplayFrame tframe = new();

                ReplayFrame rFrame = new();
                rFrame.TaikoRsDeserialize(time, reader);

                tframe.Frame = rFrame;

                frame = tframe;
                break;
            }
            case SpectatorFrameDataType.ScoreSync: {
                SpectatorFrameScoreSync tframe = new();

                tframe.Score = PlayerScore.TaikoRsDeserialize(reader);

                frame = tframe;
                break;
            }
            case SpectatorFrameDataType.ChangingMap:
                frame = new SpectatorFrameChangingMap();
                break;
            case SpectatorFrameDataType.PlayingResponse: {
                SpectatorFramePlayingResponse tframe = new();

                tframe.UserId      = reader.ReadUInt32();
                tframe.BeatmapHash = reader.ReadString();
                tframe.Mode        = reader.ReadPlayMode();
                tframe.Mods        = JsonConvert.DeserializeObject<SpectatorModInfo>(reader.ReadString());
                tframe.CurrentTime = reader.ReadSingle();

                frame = tframe;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        frame.Time = time;

        return frame;
    }
}
