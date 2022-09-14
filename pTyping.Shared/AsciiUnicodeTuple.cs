namespace pTyping.Shared;

public class AsciiUnicodeTuple : IEquatable<AsciiUnicodeTuple> {
    public readonly string? Ascii;
    public readonly string  Unicode;

    public AsciiUnicodeTuple(string? ascii, string unicode) {
        this.Ascii   = ascii;
        this.Unicode = unicode;
    }

    public AsciiUnicodeTuple(string both) {
        this.Ascii   = both;
        this.Unicode = both;
    }

    public bool Equals(AsciiUnicodeTuple? other) {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return this.Ascii == other.Ascii && this.Unicode == other.Unicode;
    }
}
