using System;
using Furball.Engine.Engine.Config;
using Furball.Volpe.Evaluation;
using pTyping.Shared.Beatmaps;

namespace pTyping.Engine;

public static class OffsetManager {
	private class OffsetConfig : VolpeConfig {
		public override string Name => "offsets";

		public static readonly OffsetConfig Instance = new OffsetConfig();
	}

	public static void Initialize() {
		OffsetConfig.Instance.Load();
	}

	public static void Save() {
		OffsetConfig.Instance.Save();
	}

	public static double GetOffset(Beatmap song) {
		if (OffsetConfig.Instance.Values.TryGetValue($"__{song.Id}", out Value offset))
			return offset.ToNumber().Value;

		return 0;
	}

	public static void SetOffset(Beatmap song, double offset) {
		offset = Math.Clamp(offset, -200, 200);

		OffsetConfig.Instance.Values[$"__{song.Id}"] = new Value.Number(offset); //TODO: move offsets to a database
	}
}
