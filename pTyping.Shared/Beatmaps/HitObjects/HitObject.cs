using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps.HitObjects;

[JsonObject(MemberSerialization.OptIn)]
public class HitObject : EmbeddedObject, IClonable<HitObject> {
	[Description("The time the hit object occurs at"), JsonProperty]
	public double Time { get; set; }

	[Description("The colour of the hit object"), JsonProperty]
	public HitObjectColor Color { get; set; }

	[Description("The text in the hit object"), JsonProperty]
	public string Text { get; set; } = "";

	[Description("The portion of the romaji the user has typed")]
	public string TypedRomaji = "";

	[Description("The portion of the whole text the user has typed")]
	public string TypedText = "";


	[Description("The result of the users hit")]
	public HitResult HitResult = HitResult.NotHit;

	[Description("Misc note specific settings"), JsonProperty]
	public HitObjectSettings Settings { get; set; } = new HitObjectSettings();

	[Ignored]
	public (string Hiragana, List<string> Romaji) TypableRomaji => this.GetTypableRomaji(this.TypedText);

	[Pure]
	public (string Hiragana, List<string> Romaji) GetTypableRomaji(string typed = "") {
		if (this.IsHit)
			return (string.Empty, new List<string> {
				string.Empty
			});

		string       textToCheck = string.Empty;
		List<string> possible    = null;

		if (this.Text.Length - typed.Length >= 3) { //Try to get the next 3 chars
			textToCheck = this.Text.Substring(typed.Length, 3);
			HiraganaConversion.CONVERSIONS.TryGetValue(textToCheck, out possible);
		}

		//Try to get the next 2 chars instead
		if (possible is null && this.Text.Length - typed.Length >= 2) {
			textToCheck = this.Text.Substring(typed.Length, 2);
			HiraganaConversion.CONVERSIONS.TryGetValue(textToCheck, out possible);
		}

		//Try to get the next char instead
		if (possible is null) {
			textToCheck = this.Text.Substring(typed.Length, 1);
			HiraganaConversion.CONVERSIONS.TryGetValue(textToCheck, out possible);
		}

		if (possible is null) throw new Exception("Unknown character! Did you put kanji? smh my head");

		possible.Sort((x, y) => x.Length - y.Length);

		return (textToCheck, possible);
	}

	[Ignored]
	public bool IsHit => this.TypedText == this.Text;

	public bool IsComplete() {
		throw new NotImplementedException();
	}
	public HitObject Clone() {
		HitObject hitObject = new HitObject {
			Color    = this.Color.Clone(),
			Settings = this.Settings.Clone(),
			Text     = this.Text,
			Time     = this.Time
		};

		return hitObject;
	}
}
