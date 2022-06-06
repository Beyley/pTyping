using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using pTyping.Graphics.Player.Mods;
using pTyping.Online;
using pTyping.Online.Tataku;
using pTyping.Songs;

namespace pTyping.Scores;

[JsonObject(MemberSerialization.OptIn)]
public class PlayerScore {
    public Song Song;

    private const ushort TATAKU_SCORE_VERSION = 5;
    [JsonProperty]
    public double Accuracy = 1d;
    [JsonProperty]
    public ushort Combo;

    [JsonProperty]
    public ushort ExcellentHits = 0;
    [JsonProperty]
    public ushort FairHits = 0;
    [JsonProperty]
    public ushort GoodHits = 0;
    [JsonProperty]
    public ushort PoorHits = 0;

    public List<PlayerMod> Mods = new();

    [JsonProperty]
    public int ReplayId = -1;

    [JsonProperty]
    public string ModsString = "";

    public bool IsOnline = false;
    
    [JsonProperty]
    public ReplayFrame[] ReplayFrames = Array.Empty<ReplayFrame>();
    public bool ReplayCheck() {
        if (this.IsOnline) return false;//TODO

        if (pTypingGame.ScoreManager.GetReplayForScore(this)) {
            if (this.ReplayFrames == null || this.ReplayFrames.Length == 0)
                return false;
        } else {
            return false;
        }

        return true;
    }

    [JsonProperty]
    public string MapHash;
    [JsonProperty]
    public ushort MaxCombo = 0;
    [JsonProperty]
    public ulong Score {
        get;
        protected set;
    }

    [JsonProperty]
    public DateTimeOffset Time;

    public void AddScore(uint score) {
        this.Score += (uint)(score * PlayerMod.ScoreMultiplier(this.Mods));
    }

    [JsonProperty]
    public string Username;

    [JsonProperty]
    public PlayMode Mode = PlayMode.pTyping;

    [JsonProperty]
    public double Speed = 1f;

    public PlayerScore(string mapHash, string username) {
        this.MapHash  = mapHash;
        this.Username = username;
    }

    public PlayerScore() {}

    [Pure]
    public static PlayerScore LoadUTypingReplay(string path) {
        FileStream   stream = File.OpenRead(path);
        BinaryReader reader = new(stream);

        PlayerScore replay = new(pTypingGame.CurrentSong.Value.MapHash, "UTyping");

        List<ReplayFrame> frames = new();

        while (stream.Position < stream.Length)
            frames.Add(ReplayFrame.UTypingDeserialize(reader));

        stream.Close();

        replay.ReplayFrames = frames.ToArray();

        return replay;
    }

    public void SaveUTypingReplay(string path) {
        FileStream   stream = File.Create(path);
        BinaryWriter writer = new(stream);

        foreach (ReplayFrame frame in this.ReplayFrames)
            frame.UTypingSerialize(writer);

        writer.Flush();
        stream.Close();
    }

    [Pure]
    public static PlayerScore TatakuDeserialize(TatakuReader reader) {
        PlayerScore score = new();

        ushort scoreVersion = reader.ReadUInt16();

        switch (scoreVersion) {
            case < 5://this is the oldest version we support
                throw new Exception("Your score version is too old!");
            case > TATAKU_SCORE_VERSION://this is the newest version we support
                throw new Exception("Your score version is too new to read!");
        }

        score.Username = reader.ReadString();
        score.MapHash  = reader.ReadString();
        
        score.Mode     = reader.ReadPlayMode();
        if (score.Mode != PlayMode.pTyping)
            throw new NotSupportedException("Wrong mode!");
        
        score.Time     = reader.ReadUnixEpoch();

        score.Score    = reader.ReadUInt64();
        score.Combo    = reader.ReadUInt16();
        score.MaxCombo = reader.ReadUInt16();

        #region Judgements

        Dictionary<string, ushort> judgements = reader.ReadStringUshortDictionary();

        score.PoorHits      = judgements["poor"];
        score.FairHits      = judgements["fair"];
        score.GoodHits      = judgements["good"];
        score.ExcellentHits = judgements["excellent"];

        #endregion
        
        score.Accuracy = reader.ReadDouble();

        //TODO: actually handle this speed
        score.Speed = reader.ReadSingle();

        //TODO: mods
        string mods = reader.ReadOptionString();

        return score;
    }

    public void TatakuSerialize(TatakuWriter writer) {
        writer.Write(TATAKU_SCORE_VERSION);

        writer.Write(this.Username);   //string
        writer.Write(this.MapHash);    //string
        writer.Write(PlayMode.pTyping);//string
        writer.Write(this.Time);       //u64 unix epoch

        writer.Write(this.Score);   //u64
        writer.Write(this.Combo);   //u16
        writer.Write(this.MaxCombo);//u16

        writer.Write(
        new Dictionary<string, ushort> {
            ["poor"]      = this.PoorHits,
            ["fair"]      = this.FairHits,
            ["good"]      = this.GoodHits,
            ["excellent"] = this.ExcellentHits
        }
        );//Dictionary<string, ushort>

        writer.Write(this.Accuracy);// f64
        writer.Write(this.Speed);   // f32

        // TODO: mods
        writer.WriteOptionString(null);//string 
    }

    [Pure]
    public byte[] TatakuSerialize() {
        using MemoryStream stream = new();
        using TatakuWriter writer = new(stream);

        this.TatakuSerialize(writer);
        
        writer.Flush();

        return stream.ToArray();
    }
}

[JsonObject(MemberSerialization.OptIn)]
public struct ReplayFrame {
    [JsonProperty]
    public char Character;
    [JsonProperty]
    public double Time;

    public bool Used;

    [Pure]
    public static ReplayFrame UTypingDeserialize(BinaryReader reader) {
        ReplayFrame frame = new();

        frame.Character = reader.ReadChar();
        frame.Time      = reader.ReadDouble() * 1000d;
        frame.Used      = false;

        return frame;
    }

    public void UTypingSerialize(BinaryWriter writer) {
        writer.Write(this.Character);
        writer.Write(this.Time / 1000d);
    }

    public void TatakuSerialize(BinaryWriter writer) {
        writer.Write((byte)2);//key press

        // We write this weirdly as the `MousePos` type requires 64bits of data
        // (this will be parsed as 2 floats on the server and other clients)
        writer.Write(this.Character);        // 8 bits
        writer.Write((byte)0);               // 8 bits
        writer.Write(short.MaxValue);        // 16 bits
        writer.Write(float.NegativeInfinity);// 32 bits
    }

    public void TatakuDeserialize(double time, BinaryReader reader) {
        this.Time      = time;
        reader.ReadByte();
        this.Character = reader.ReadChar();

        reader.ReadBytes(7);//extra garbage we ignore
    }
}