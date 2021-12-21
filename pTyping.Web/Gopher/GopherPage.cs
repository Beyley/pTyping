using System.Collections.Generic;

namespace pTyping.Web.Gopher;

public abstract class GopherPage {
    public abstract List<GopherItem> Items { get; }

    public static GopherPage GetPage(string path) {
        return path switch {
            GopherPageIndex.PATH => GopherPageIndex.Instance,
            _                    => GopherPageNotFound.Instance
        };

    }

    public class GopherPageIndex : GopherPage {
        public const string PATH = "/";

        public static readonly GopherPage Instance = new GopherPageIndex();

        public override List<GopherItem> Items => new() {
            new GopherItemInformationMessage("Welcome to the gopherspace for the rhythm game pTyping!"),
            new GopherItemInformationMessage("pTyping is a clone of the japanese only game UTyping"),
            new GopherItemHtmlFile("UTyping home page (Japanese)", "https://tosk.jp/utyping/"),
            new GopherItemHtmlFile("pTyping Github",               "https://github.com/Beyley/pTyping"),
            new GopherItemHtmlFile("pTyping Website",              "https://typing.beyleyisnot.moe")
        };
    }

    public class GopherPageNotFound : GopherPage {
        public static readonly GopherPage Instance = new GopherPageNotFound();

        public override List<GopherItem> Items => new() {
            new GopherItemInformationMessage("Im not sure how you found your way here, maybe a dead link or mistype?"),
            new GopherItemInformationMessage("In any case, you should head back to the index!"),
            new GopherItemInformationMessage("(and maybe give a stern talking to who linked you here)"),
            new GopherItemFile("Root", "/", Server.GOPHER_IP, Server.GOPHER_PORT)
        };
    }
}
