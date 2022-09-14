#nullable enable
using System.ComponentModel;
using Realms;

namespace pTyping.Shared;

public class AsciiUnicodeTuple : RealmObject, IEquatable<AsciiUnicodeTuple> {
    [Description("The raw Ascii version of the text.")]
    public string? Ascii { get; set; }
    [Description("The Unicode version of the text.")]
    public string Unicode { get; set; }

    public AsciiUnicodeTuple(string? ascii, string unicode) {
        this.Ascii   = ascii;
        this.Unicode = unicode;
    }

    public AsciiUnicodeTuple(string both) {
        this.Ascii   = both;
        this.Unicode = both;
    }

    public AsciiUnicodeTuple() => this.Unicode = "";

    public bool Equals(AsciiUnicodeTuple? other) {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return this.Ascii == other.Ascii && this.Unicode == other.Unicode;
    }
}
