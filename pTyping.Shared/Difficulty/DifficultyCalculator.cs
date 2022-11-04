using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.HitObjects;

namespace pTyping.Shared.Difficulty;

public class DifficultyCalculator {
	private readonly Beatmap _beatmap;

	public DifficultyCalculator(Beatmap beatmap) {
		this._beatmap = beatmap;
	}

	public double Calculate() {
		//Iterate all notes and get the average time between them
		double averageTimeBetweenNotes = 0;
		double lastTime                = 0;
		int    noteCount               = 0;
		foreach (HitObject note in this._beatmap.HitObjects) {
			//get the distance between the last note and this one
			double distance = note.Time - lastTime;

			lastTime = note.Time;

			//Dont count the first note, as you always have infinite time to prepare your hands
			if (lastTime == 0)
				continue;

			//ignore notes that are more than 5 seconds apart, as theres ample time to prepare
			if (distance > 5000)
				continue;

			double weight = 0;

			foreach (char hiragana in note.Text) {
				if (TypingConversions.Conversions[note.TypingConversion].TryGetValue(hiragana.ToString(), out List<string> conversion)) {
					if (conversion.Count == 0) {
						weight++;
						continue;
					}

					conversion.Sort((x, y) => string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase));

					weight += conversion[0].Length;
				}
				else {
					weight++;
				}
			}

			// weight = weight.Clamp(0, 10);

			averageTimeBetweenNotes += distance / (weight / 2d);
			noteCount++;
		}
		// averageTimeBetweenNotes /= noteCount;

		return 10000 / (averageTimeBetweenNotes / noteCount);
	}
}
