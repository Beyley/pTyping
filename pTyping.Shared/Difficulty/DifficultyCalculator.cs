using pTyping.Shared.Beatmaps;

namespace pTyping.Shared.Difficulty;

public class DifficultyCalculator {
	private readonly Beatmap _beatmap;

	public DifficultyCalculator(Beatmap beatmap) {
		this._beatmap = beatmap;
	}

	public double Calculate() {
		return 1;
	}
}
