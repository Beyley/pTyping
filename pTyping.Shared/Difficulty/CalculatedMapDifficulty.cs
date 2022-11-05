using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Difficulty;

public class CalculatedMapDifficulty : EmbeddedObject, ICloneable<CalculatedMapDifficulty> {
	public CalculatedMapDifficulty() {
		this.OverallDifficulty = 0;
		this.Sections          = Array.Empty<DifficultySection>();
	}

	public CalculatedMapDifficulty(double overallDifficulty, IList<DifficultySection> sections) {
		this.OverallDifficulty = overallDifficulty;
		this.Sections          = sections;
	}

	public double                   OverallDifficulty { get; set; }
	public IList<DifficultySection> Sections          { get; }

	public CalculatedMapDifficulty Clone() => new CalculatedMapDifficulty(this.OverallDifficulty, this.Sections);
}
