using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using pTyping.Shared.Mods;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Scores;

[JsonObject(MemberSerialization.OptIn)]
public class Score : RealmObject, ICloneable<Score> {
	public Score() {
		this.User = new DatabaseUser();
		this.Id   = Guid.NewGuid();
	}

	[JsonProperty, Description("A unique ID for the score"), PrimaryKey]
	public Guid Id { get; set; }

	[JsonProperty, Description("The user that made the score")]
	public DatabaseUser User { get; set; }

	[JsonProperty, MapTo("Score"), Description("The score the user achieved")]
	public long AchievedScore { get; set; }

	[JsonProperty, Description("The accuracy the user achieved")]
	public double Accuracy { get; set; }

	[JsonProperty, Description("The maximum combo the user achieved")]
	public int MaxCombo { get; set; }

	[JsonIgnore, Ignored]
	public int CurrentCombo { get; set; }

	[JsonProperty, Description("The amount of excellent hits the user achieved")]
	public int ExcellentHits { get; set; }
	[JsonProperty, Description("The amount of fair hits the user achieved")]
	public int FairHits { get; set; }
	[JsonProperty, Description("The amount of good hits the user achieved")]
	public int GoodHits { get; set; }
	[JsonProperty, Description("The amount of poor hits the user achieved")]
	public int PoorHits { get; set; }

	[JsonProperty, Description("The mods the user used for the play"), MapTo("Mods")]
	public string ModsJson { get; set; } //This is stored as a JSON string

	[CanBeNull]
	private Mod[] _mods;

	private void UpdateModsJson() {
		if (this.Realm != null)
			this.Realm.Write(() => {
				this.ModsJson = JsonConvert.SerializeObject(
					this._mods,
					new JsonSerializerSettings {
						TypeNameHandling = TypeNameHandling.All
					}
				);
			});
		else
			this.ModsJson = JsonConvert.SerializeObject(
				this._mods,
				new JsonSerializerSettings {
					TypeNameHandling = TypeNameHandling.All
				}
			);
	}

	[Ignored, Description("All the mods the score used, this is a frontend ease property for the backing ModsJson property.")]
	public Mod[] Mods {
		get {
			if (this._mods != null)
				return this._mods;

			if (string.IsNullOrEmpty(this.ModsJson)) {
				this._mods = Array.Empty<Mod>();
				this.UpdateModsJson();

				//NOTE: this isn't required, but saves us an extra JSON deserialization
				return Array.Empty<Mod>();
			}

			//Deserialize the mod JSON
			this._mods = JsonConvert.DeserializeObject<Mod[]>(this.ModsJson!, new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.All
			});

			return this._mods;
		}
		set {
			this._mods = value;

			this.UpdateModsJson();
		}
	}

	[JsonProperty, Description("A flag telling whether the score originated from another user online.")]
	public bool OnlineScore { get; set; }

	[JsonProperty, Description("A list of replay frames associated with this score")]
	public IList<ReplayFrame> ReplayFrames { get; }

	[JsonProperty, Description("The unique ID of the beatmap")]
	public string BeatmapId { get; set; }

	[JsonProperty, Description("The time the score was set")]
	public DateTimeOffset Time { get; set; }

	public void AddScore(long i) {
		this.AchievedScore += i; //TODO: mod multipliers
	}
	public Score Clone() {
		Score score = new Score {
			Time          = this.Time,
			Accuracy      = this.Accuracy,
			Mods          = this.Mods,
			User          = this.User.Clone(),
			AchievedScore = this.AchievedScore,
			BeatmapId     = this.BeatmapId,
			CurrentCombo  = this.CurrentCombo,
			ExcellentHits = this.ExcellentHits,
			FairHits      = this.FairHits,
			GoodHits      = this.GoodHits,
			MaxCombo      = this.MaxCombo,
			OnlineScore   = this.OnlineScore,
			PoorHits      = this.PoorHits,
			Id            = this.Id,
			ModsJson      = this.ModsJson
		};

		foreach (ReplayFrame frame in this.ReplayFrames)
			score.ReplayFrames.Add(frame.Clone());

		return score;
	}
}
