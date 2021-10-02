using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace pTyping.Songs {
	public enum HitResult {
		Excellent,
		Good,
		Fair,
		Poor,
		Miss,
		NotHit
	}
	
	[JsonObject(MemberSerialization.OptIn)]
	public class Note {
		public List<string> ThisCharacterRomaji {
			get {
				if (this.IsHit) return new() {
					string.Empty
				};
				
				HiraganaConversion.Conversions.TryGetValue(this.Text[this.Typed.Length].ToString(), out List<string> possible);

				possible.Sort((x, y) => x.Length - y.Length);
				
				return possible;
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

		public bool IsHit => this.Typed == this.Text;

		/// <summary>
		/// Whether or not the note has been hit
		/// </summary>
		public HitResult HitResult = HitResult.NotHit;
		/// <summary>
		/// The currently typed part of the note
		/// </summary>
		public string Typed = "";
		/// <summary>
		/// The currently typed part of the current hiragana to type
		/// </summary>
		public string TypedRomaji = "";
	}
}
