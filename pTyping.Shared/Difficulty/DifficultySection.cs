using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Difficulty;

public class DifficultySection : EmbeddedObject, ICloneable<DifficultySection> {
	public DifficultySection() {
		this.Difficulty = 0;
		this.NoteCount  = 0;
	}

	public DifficultySection(double difficulty, int noteCount) {
		this.Difficulty = difficulty;
		this.NoteCount  = noteCount;
	}

	public double Difficulty { get; set; }
	public int    NoteCount  { get; set; }

	public DifficultySection Clone() => new DifficultySection(this.Difficulty, this.NoteCount);
}
