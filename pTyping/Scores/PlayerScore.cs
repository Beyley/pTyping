using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using pTyping.Online;
using pTyping.Online.Tataku;
using pTyping.Shared.ObjectModel;
using pTyping.Shared.Scores;

namespace pTyping.Scores;

[JsonObject(MemberSerialization.OptIn)]
public static class ScoreExtensions {
	private const ushort TATAKU_SCORE_VERSION = 5;

	public static Score LoadUTypingReplay(byte[] arr) {
		MemoryStream stream = new MemoryStream(arr);
		BinaryReader reader = new BinaryReader(stream);

		Score replay = new Score {
			User = new DatabaseUser {
				Username = "UTyping",
				UserId   = uint.MaxValue
			}
		};

		List<ReplayFrame> frames = new List<ReplayFrame>();

		while (stream.Position < stream.Length)
			frames.Add(ReplayFrameExtensions.UTypingDeserialize(reader));

		stream.Close();

		foreach (ReplayFrame x in frames)
			replay.ReplayFrames.Add(x);

		return replay;
	}

	[Pure]
	public static Score LoadUTypingReplay(string path) {
		return LoadUTypingReplay(File.ReadAllBytes(path));
	}

	public static void SaveUTypingReplay(this Score score, string path) {
		FileStream   stream = File.Create(path);
		BinaryWriter writer = new BinaryWriter(stream);

		foreach (ReplayFrame frame in score.ReplayFrames)
			frame.UTypingSerialize(writer);

		writer.Flush();
		stream.Close();
	}

	[Pure]
	public static Score TatakuDeserialize(TatakuReader reader) {
		Score score = new Score();

		ushort scoreVersion = reader.ReadUInt16();

		switch (scoreVersion) {
			case < 5: //this is the oldest version we support
				throw new Exception("Your score version is too old!");
			case > TATAKU_SCORE_VERSION: //this is the newest version we support
				throw new Exception("Your score version is too new to read!");
		}

		score.User.Username = reader.ReadString();

		score.BeatmapId = reader.ReadString();

		if (reader.ReadPlayMode() != PlayMode.pTyping)
			throw new NotSupportedException("Wrong mode!");

		score.Time = reader.ReadUnixEpoch();

		score.AchievedScore = (long)reader.ReadUInt64();
		_                   = reader.ReadUInt16(); //ignore Combo
		score.MaxCombo      = reader.ReadUInt16();

		#region Judgements

		Dictionary<string, ushort> judgements = reader.ReadStringUshortDictionary();

		score.PoorHits      = judgements["poor"];
		score.FairHits      = judgements["fair"];
		score.GoodHits      = judgements["good"];
		score.ExcellentHits = judgements["excellent"];

		#endregion

		score.Accuracy = reader.ReadDouble();

		//TODO: actually handle this speed
		_ = reader.ReadSingle(); //IGNORE SPEED

		//TODO: mods
		string mods = reader.ReadOptionString();

		return score;
	}

	public static void TatakuSerialize(this Score score, TatakuWriter writer) {
		writer.Write(TATAKU_SCORE_VERSION);

		writer.Write(score.User.Username); //string
		writer.Write(score.BeatmapId);     //string
		writer.Write(PlayMode.pTyping);    //string
		writer.Write(score.Time);          //u64 unix epoch

		writer.Write((ulong)score.AchievedScore); //u64
		writer.Write(0);                          //u16 we dont store this information, should be Combo
		writer.Write((ushort)score.MaxCombo);     //u16

		writer.Write(
			new Dictionary<string, ushort> {
				["poor"]      = (ushort)score.PoorHits,
				["fair"]      = (ushort)score.FairHits,
				["good"]      = (ushort)score.GoodHits,
				["excellent"] = (ushort)score.ExcellentHits
			}
		); //Dictionary<string, ushort>

		writer.Write(score.Accuracy); // f64
		writer.Write(1f);             // f32 TODO: speed

		// TODO: mods
		writer.WriteOptionString(null); //string 
	}

	[Pure]
	public static byte[] TatakuSerialize(this Score score) {
		using MemoryStream stream = new MemoryStream();
		using TatakuWriter writer = new TatakuWriter(stream);

		score.TatakuSerialize(writer);

		writer.Flush();

		return stream.ToArray();
	}
}

public static class ReplayFrameExtensions {
	[Pure]
	public static ReplayFrame UTypingDeserialize(BinaryReader reader) {
		ReplayFrame frame = new ReplayFrame {
			Character = reader.ReadChar(),
			Time      = reader.ReadDouble() * 1000d
		};

		return frame;
	}

	public static void UTypingSerialize(this ReplayFrame frame, BinaryWriter writer) {
		writer.Write(frame.Character);
		writer.Write(frame.Time / 1000d);
	}

	public static void TatakuSerialize(this ReplayFrame frame, BinaryWriter writer) {
		writer.Write((byte)2); //key press

		// We write this weirdly as the `MousePos` type requires 64bits of data
		// (this will be parsed as 2 floats on the server and other clients)
		writer.Write(frame.Character);        // 8 bits
		writer.Write((byte)0);                // 8 bits
		writer.Write(short.MaxValue);         // 16 bits
		writer.Write(float.NegativeInfinity); // 32 bits
	}

	public static void TatakuDeserialize(this ReplayFrame frame, double time, BinaryReader reader) {
		frame.Time = time;
		reader.ReadByte();
		frame.Character = reader.ReadChar();

		reader.ReadBytes(7); //extra garbage we ignore
	}
}
