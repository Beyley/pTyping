using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace pTyping.Songs {
	public enum HitResult {
		Unknown,
		Hit,
		Miss
	}
	[JsonObject(MemberSerialization.OptIn)]
	public class Note {
		[JsonProperty]
		public string TextToType;
		[JsonProperty]
		public string TextToShow;
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
