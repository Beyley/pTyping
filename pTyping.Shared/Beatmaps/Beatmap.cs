using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Events;
using pTyping.Shared.ObjectModel;
using Realms;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace pTyping.Shared.Beatmaps;

[JsonObject(MemberSerialization.OptIn)]
public class Beatmap : RealmObject, ICloneable<Beatmap>, IEquatable<Beatmap>, ICopyable<Beatmap> {
	[PrimaryKey, JsonProperty]
	public string Id { get; set; }

	[Backlink(nameof (BeatmapSet.Beatmaps))]
	public IQueryable<BeatmapSet> Parent { get; }

	[Ignored, JsonIgnore]
	public BeatmapSet ParentSet => this.Parent.First();

	[Description("All of the breaks that happen during the Beatmap."), JsonProperty]
	public IList<Break> Breaks { get; }
	[Description("All of the objects contained within the Beatmap."), JsonProperty]
	public IList<HitObject> HitObjects { get; }
	[Description("All of the events container within the Beatmap."), JsonProperty]
	public IList<Event> Events { get; }
	[Description("All of the timing points within the Beatmap."), JsonProperty]
	public IList<TimingPoint> TimingPoints { get; }

	[JsonProperty]
	public BeatmapDifficulty Difficulty { get; set; }
	[JsonProperty]
	public BeatmapInfo Info { get; set; }
	[JsonProperty]
	public BeatmapMetadata Metadata { get; set; }
	[JsonProperty]
	public BeatmapFileCollection FileCollection { get; set; }

	public Beatmap() {
		this.Info = new BeatmapInfo {
			Description    = "",
			Mapper         = new DatabaseUser("Unknown"),
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

	[Ignored, Description("The BPM of the first timing point of the song"), JsonIgnore]
	public double BeatsPerMinute => this.TimingPoints[0].BeatsPerMinute;

	public Beatmap Clone() {
		Beatmap map = new Beatmap();

		foreach (Break @break in this.Breaks)
			map.Breaks.Add(@break.Clone());
		map.Difficulty.Strictness = this.Difficulty.Strictness;
		foreach (Event @event in this.Events)
			map.Events.Add(@event.Clone());
		map.Id             = this.Id;
		map.Info           = this.Info.Clone();
		map.Metadata       = this.Metadata.Clone();
		map.FileCollection = this.FileCollection.Clone();
		// map.Parent         = this.Parent;
		foreach (HitObject hitObject in this.HitObjects)
			map.HitObjects.Add(hitObject.Clone());
		foreach (TimingPoint timingPoint in this.TimingPoints)
			map.TimingPoints.Add(timingPoint.Clone());

		return map;
	}

	public void CopyInto(Beatmap beatmap) {
		beatmap.Breaks.Clear();
		foreach (Break @break in this.Breaks)
			beatmap.Breaks.Add(@break.Clone());
		beatmap.Difficulty = this.Difficulty.Clone();
		beatmap.Events.Clear();
		foreach (Event @event in this.Events)
			beatmap.Events.Add(@event.Clone());
		beatmap.Id             = this.Id;
		beatmap.Info           = this.Info.Clone();
		beatmap.Metadata       = this.Metadata.Clone();
		beatmap.FileCollection = this.FileCollection.Clone();
		beatmap.HitObjects.Clear();
		foreach (HitObject hitObject in this.HitObjects)
			beatmap.HitObjects.Add(hitObject);
		beatmap.TimingPoints.Clear();
		foreach (TimingPoint timingPoint in this.TimingPoints)
			beatmap.TimingPoints.Add(timingPoint);
	}

	public bool AllNotesHit() {
		return this.HitObjects.All(x => x.Complete);
	}

	public TimingPoint CurrentTimingPoint(double time) {
		if (this.TimingPoints.Count == 0)
			return new TimingPoint(0, 100);

		return this.TimingPoints.FirstOrDefault(x => x.Time <= time, this.TimingPoints[0]);
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
