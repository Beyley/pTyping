using System.Collections.Generic;
using System.Text;

namespace pTyping.Web.Gopher;

public struct GopherResponse {
    public List<GopherItem> Items;

    public GopherResponse(List<GopherItem> items) => this.Items = items;

    public string GetResult() {
        StringBuilder builder = new();

        foreach (GopherItem item in this.Items)
            builder.Append($"{item.GetLine()}\r\n");

        builder.Append('.');

        return builder.ToString();
    }
}
