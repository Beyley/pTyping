using System;
using System.Diagnostics.CodeAnalysis;

namespace pTyping.Web.Gopher;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class GopherLineType {

    /// <summary>
    ///     A regular file on the server
    /// </summary>
    public static readonly GopherLineType File = new("0");
    /// <summary>
    ///     A directory on the server
    /// </summary>
    public static readonly GopherLineType Directory = new("1");
    /// <summary>
    ///     A CSO phone-book server
    /// </summary>
    public static readonly GopherLineType CSOPhoneBookServer = new("2");
    /// <summary>
    ///     An error
    /// </summary>
    public static readonly GopherLineType Error = new("3");
    /// <summary>
    ///     A BixHexed machintosh file
    /// </summary>
    public static readonly GopherLineType BinHexedMacintoshFile = new("4");
    /// <summary>
    ///     A DOS binary archive of some sort
    /// </summary>
    /// <remarks>
    ///     This will cause the client to read until the connection closes
    /// </remarks>
    public static readonly GopherLineType DOSBinaryArchive = new("5");
    public static readonly GopherLineType UNIXUUEncodedFile      = new("6");
    public static readonly GopherLineType IndexSearchServer      = new("7");
    public static readonly GopherLineType TextBasedTelnetSession = new("8");
    /// <summary>
    ///     A normal binary file
    /// </summary>
    /// <remarks>
    ///     This will cause the client to read until the connection closes
    /// </remarks>
    public static readonly GopherLineType BinaryFile = new("9");
    public static readonly GopherLineType RedundantServer            = new("+");
    public static readonly GopherLineType TextBasedTelnet3270Session = new("T");
    public static readonly GopherLineType GIFFormatGraphicsFile      = new("g");
    /// <remarks>
    ///     Rendering is determined by the client
    /// </remarks>
    public static readonly GopherLineType ImageFile = new("I");
    public static readonly GopherLineType HtmlFile = new("h");
    /// <summary>
    ///     Usually used alongside .PDF and .DOC files
    /// </summary>
    public static readonly GopherLineType Doc = new("d");
    /// <summary>
    ///     Information message, basically just a normal line of text to display on the client
    /// </summary>
    public static readonly GopherLineType InformationMessage = new("i");
    /// <summary>
    ///     Usually in WAV format
    /// </summary>
    public static readonly GopherLineType SoundFile = new("s");
    public string Character;
    public bool   ClientReadsToEnd;

    public GopherLineType(string character, bool clientReadsToEnd = false) {
        this.Character        = character;
        this.ClientReadsToEnd = clientReadsToEnd;
    }
}

public abstract class GopherItem {
    public abstract GopherLineType Type { get; }
    /// <summary>
    ///     Gets the line to send to the client
    /// </summary>
    /// <returns>The finished line, this should NOT contain the newline at the end, nor the period</returns>
    public abstract string GetLine();
}

public class GopherItemInformationMessage : GopherItem {

    public string Text;

    public GopherItemInformationMessage(string text) {
        this.Text = text;

        if (this.Text.Contains('\n'))
            throw new ArgumentException("Text *cannot* contain newlines!");

        if (text.Length >= 80)
            throw new ArgumentException("Text lines too long will be truncated by most clients! Split the line up please!");
    }
    public override GopherLineType Type => GopherLineType.InformationMessage;

    public override string GetLine() => $"{this.Type.Character}{this.Text}\t\terror.host\t1";
}

public class GopherItemFile : GopherItem {
    public string Address;
    public string Path;
    public short  Port;
    public string Text;

    public GopherItemFile(string text, string path, string address, short port) {
        this.Path = path;
        this.Text = text;
        if (text.Length >= 80)
            throw new ArgumentException("Text lines too long will be truncated by most clients! Split the line up please!");
        if (this.Text.Contains('\n'))
            throw new ArgumentException("Text *cannot* contain newlines!");
        this.Address = address;
        this.Port    = port;
    }
    public override GopherLineType Type => GopherLineType.File;

    public override string GetLine() => $"{this.Type.Character}{this.Text}\t{this.Path}\t{this.Address}\t{this.Port}";
}

public class GopherItemHtmlFile : GopherItem {
    public string Path;
    public string Text;

    public GopherItemHtmlFile(string text, string path) {
        this.Path = path;
        this.Text = text;

        if (text.Length >= 80)
            throw new ArgumentException("Text lines too long will be truncated by most clients! Split the line up please!");
        if (this.Text.Contains('\n'))
            throw new ArgumentException("Text *cannot* contain newlines!");
    }
    public override GopherLineType Type => GopherLineType.HtmlFile;

    public override string GetLine() => $"{this.Type.Character}{this.Text}\tURL:{this.Path}\t{Server.GOPHER_IP}\t{Server.GOPHER_PORT}";
}
