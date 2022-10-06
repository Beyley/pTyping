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

	[Description("The typing conversion to use for this hit object"), JsonProperty, Ignored]
	public TypingConversions.ConversionType TypingConversion {
		get => (TypingConversions.ConversionType)this._backingTypingConversion;
		set => this._backingTypingConversion = (int)value;
	}

	private int _backingTypingConversion { get; set; } = (int)TypingConversions.ConversionType.StandardHiragana;

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
			TypingConversions.Conversions[this.TypingConversion].TryGetValue(textToCheck, out possible);
		}

		//Try to get the next 2 chars instead
		if (possible is null && this.Text.Length - typed.Length >= 2) {
			textToCheck = this.Text.Substring(typed.Length, 2);
			TypingConversions.Conversions[this.TypingConversion].TryGetValue(textToCheck, out possible);
		}

		//Try to get the next char instead
		if (possible is null) {
			textToCheck = this.Text.Substring(typed.Length, 1);
			TypingConversions.Conversions[this.TypingConversion].TryGetValue(textToCheck, out possible);
		}

		if (possible is null) return (textToCheck, new List<string> { textToCheck });

		possible.Sort((x, y) => x.Length - y.Length);

		return (textToCheck, possible);
	}

	[Ignored]
	public bool IsHit => this.TypedText == this.Text;

	public HitObject Clone() {
		HitObject hitObject = new HitObject {
			Color            = this.Color.Clone(),
			Settings         = this.Settings.Clone(),
			Text             = this.Text,
			Time             = this.Time,
			TypingConversion = this.TypingConversion
		};

		return hitObject;
	}
}
