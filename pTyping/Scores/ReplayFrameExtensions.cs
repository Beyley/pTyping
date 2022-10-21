using System.IO;
using JetBrains.Annotations;
using pTyping.Shared.Scores;

namespace pTyping.Scores;

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
