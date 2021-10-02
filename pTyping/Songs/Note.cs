using Newtonsoft.Json;
using Microsoft.Xna.Framework;

namespace pTyping.Songs {
	public enum HitResult {
		Unknown,
		Hit,
		Miss
	}
	[JsonObject(MemberSerialization.OptIn)]
	public class Note {
		public string TextToType {
			get {
				string toType = string.Empty;

				for (int i = 0; i < this.Text.Length; i++) {
					string currentCharacter = this.Text[i].ToString();

					foreach (Conversion conversion in HiraganaConversion.Conversions) {
						if (currentCharacter == conversion.Hiragana) {
							toType += conversion.Romaji;
							break;
						}
					}
				}

				return toType;
			}
		}
		[JsonProperty]
		public string Text;
		[JsonProperty]
		public double Time;
		[JsonProperty]
		public Color Color = Color.Red;
		[JsonProperty]
		public float YOffset;

		/// <summary>
		/// Whether or not the note has been hit
		/// </summary>
		public HitResult Hit = HitResult.Unknown;
		public double HitAmount => (double)this.Typed.Length / (double)this.TextToType.Length;
		/// <summary>
		/// The currently typed part of the note
		/// </summary>
		public string Typed = "";

		public char NextToType => this.TextToType[this.Typed.Length];
	}
}
