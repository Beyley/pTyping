using Newtonsoft.Json;

namespace pTyping.Songs {
	[JsonObject(MemberSerialization.OptIn)]
	public abstract class Event {
		[JsonProperty]
		public abstract EventType Type { get; }
		[JsonProperty]
		public double Time { get; set; }
	}

	public enum EventType {
		Rest,
		Lyric,
		TypingCutoff
	}
}
