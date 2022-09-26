using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Events;

public class Event : EmbeddedObject, IClonable<Event>, IComparable<Event> {
	[Description("The end of the event")]
	public double End { get; set; }
	[Description("The start of the event")]
	public double Start { get; set; }

	[Description("The text of the lyric event")]
	public string Text { get; set; }

	public int BackingType { get; set; }
	[Ignored, Description("The type of event")]
	public EventType Type {
		get => (EventType)this.BackingType;
		set => this.BackingType = (int)value;
	}

	[Ignored, Description("The length of the event")]
	public double Length => this.End - this.Start;

	public Event Clone() {
		Event @event = new Event {
			Start = this.Start,
			End   = this.End,
			Text  = this.Text,
			Type  = this.Type
		};

		return @event;
	}

	public int CompareTo(Event other) {
		if (ReferenceEquals(this, other))
			return 0;
		if (ReferenceEquals(null, other))
			return 1;
		int endComparison = this.End.CompareTo(other.End);
		if (endComparison != 0)
			return endComparison;
		int startComparison = this.Start.CompareTo(other.Start);
		if (startComparison != 0)
			return startComparison;
		int textComparison = string.Compare(this.Text, other.Text, StringComparison.Ordinal);
		if (textComparison != 0)
			return textComparison;
		return this.BackingType.CompareTo(other.BackingType);
	}
}
