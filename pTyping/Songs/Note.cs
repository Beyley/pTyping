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
		public int Time;

		/// <summary>
		/// Whether or not the note has been hit
		/// </summary>
		public HitResult Hit = HitResult.Unknown;
		/// <summary>
		/// The currently typed part of the note
		/// </summary>
		public string Typed = "";

		public char NextToType => this.TextToType[this.Typed.Length];
	}
}
