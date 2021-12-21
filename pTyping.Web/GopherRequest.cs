using System.Text;

namespace pTyping.Web;

public struct GopherRequest {
    public string Location;

    public GopherRequest(byte[] data) {
        string request = Encoding.UTF8.GetString(data);

        this.Location = request.GetUntil(request.Contains('\t') ? "\t" : "\r\n");
        if (string.IsNullOrWhiteSpace(this.Location))
            this.Location = "/";
    }
}
