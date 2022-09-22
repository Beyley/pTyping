using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Events;

public class Event : EmbeddedObject, IClonable<Event> {
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
        Event @event = new() {
            Start = this.Start,
            End   = this.End,
            Text  = this.Text,
            Type  = this.Type
        };

        return @event;
    }
}
