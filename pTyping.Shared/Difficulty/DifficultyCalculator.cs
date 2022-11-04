using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.HitObjects;

namespace pTyping.Shared.Difficulty;

public class DifficultyCalculator {
	private readonly Beatmap _beatmap;

	public DifficultyCalculator(Beatmap beatmap) {
		this._beatmap = beatmap;
	}

	public CalculatedMapDifficulty Calculate() {
		//If theres no notes, dont do anything, just return 0
		if (this._beatmap.HitObjects.Count == 0)
			return new CalculatedMapDifficulty(0, Array.Empty<DifficultySection>());

		//Get the first hit object
		HitObject firstObject = this._beatmap.HitObjects[0];

		//Get the last hit object
		HitObject lastObject = this._beatmap.HitObjects.Last();

		const double spacingBetweenSections = 1000; //1 second (in milliseconds)

		//Get the amount of sections
		int sectionCount = (int)Math.Ceiling((lastObject.Time - firstObject.Time) / spacingBetweenSections);

		int GetSectionFromTime(double time) {
			return (int)Math.Floor((time - firstObject.Time) / spacingBetweenSections);
		}

		//Create the sections
		DifficultySection[] sections = new DifficultySection[sectionCount];

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

			//Get the section this note belongs to
			int section = GetSectionFromTime(note.Time);

			sections[section] ??= new DifficultySection(0, 0);
			
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
			sections[section].Difficulty += distance / (weight / 2d);
			sections[section].NoteCount++;
		}

		for (int i = 0; i < sections.Length; i++) {
			DifficultySection section = sections[i] ?? (sections[i] = new DifficultySection());

			if (section.NoteCount == 0) {
				section.Difficulty = 0;
				continue;
			}

			section.Difficulty = 10000 / (section.Difficulty / section.NoteCount);
		}

		return new CalculatedMapDifficulty(10000 / (averageTimeBetweenNotes / noteCount), sections);
	}
}
