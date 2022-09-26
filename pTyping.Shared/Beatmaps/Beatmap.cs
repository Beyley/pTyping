using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Events;
using pTyping.Shared.ObjectModel;
using Realms;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace pTyping.Shared.Beatmaps;

public class Beatmap : RealmObject, IClonable<Beatmap>, IEquatable<Beatmap> {
	[PrimaryKey]
	public string Id { get; set; }

	public BeatmapSet Parent { get; set; }

	[Description("All of the breaks that happen during the Beatmap.")]
	public IList<Break> Breaks { get; }
	[Description("All of the objects contained within the Beatmap.")]
	public IList<HitObject> HitObjects { get; }
	[Description("All of the events container within the Beatmap.")]
	public IList<Event> Events { get; }
	[Description("All of the timing points within the Beatmap.")]
	public IList<TimingPoint> TimingPoints { get; }

	public BeatmapDifficulty     Difficulty     { get; set; }
	public BeatmapInfo           Info           { get; set; }
	public BeatmapMetadata       Metadata       { get; set; }
	public BeatmapFileCollection FileCollection { get; set; }

	public Beatmap() {
		this.Info = new BeatmapInfo {
			Description    = "",
			Mapper         = "Unknown Creator",
			DifficultyName = new AsciiUnicodeTuple(""),
			PreviewTime    = 0
		};
		this.Difficulty = new BeatmapDifficulty();

		this.Metadata = new BeatmapMetadata();
		this.Metadata.BackingLanguages.Add((int)SongLanguage.Unknown);

		this.FileCollection = new BeatmapFileCollection {
			Audio           = null,
			Background      = null,
			BackgroundVideo = null
		};
	}

	[Description("The total duration of all the breaks in this Beatmap."), JsonIgnore]
	public double TotalBreakDuration => this.Breaks.Sum(b => b.Length);

	[Ignored, Description("The BPM of the first timing point of the song")]
	public double BeatsPerMinute => 60000 / this.TimingPoints[0].Tempo;

	public Beatmap Clone() {
		Beatmap map = new();

		foreach (Break @break in this.Breaks)
			map.Breaks.Add(@break.Clone());
		map.Difficulty.Strictness = this.Difficulty.Strictness;
		foreach (Event @event in this.Events)
			map.Events.Add(@event.Clone());
		map.Id             = this.Id;
		map.Info           = this.Info.Clone();
		map.Metadata       = this.Metadata.Clone();
		map.FileCollection = this.FileCollection.Clone();
		foreach (HitObject hitObject in this.HitObjects)
			map.HitObjects.Add(hitObject.Clone());
		foreach (TimingPoint timingPoint in this.TimingPoints)
			map.TimingPoints.Add(timingPoint.Clone());

		return map;
	}

	public bool AllNotesHit() {
		return this.HitObjects.All(x => x.HitResult != HitResult.NotHit);
	}

	public TimingPoint CurrentTimingPoint(double time) {
		return this.TimingPoints.First(x => x.Time <= time);
	}

	[Pure]
	public double DividedNoteLength(double currentTime) {
		TimingPoint timingPoint = this.CurrentTimingPoint(currentTime);

		return timingPoint.Tempo / timingPoint.TimeSignature;
	}

	public bool Equals(Beatmap other) {
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;

		return base.Equals(other) && this.Id == other.Id;
	}
}
