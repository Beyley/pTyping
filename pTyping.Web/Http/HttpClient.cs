using System.Collections.Generic;
using System.IO;
using System.Text;
using EeveeTools.Servers.TCP;

namespace pTyping.Web.Http;

public class HttpClient : TcpClientHandler {
    private readonly List<byte> _data = new();
    
    protected override void HandleData(byte[] data) {
        this._data.AddRange(data);
        if (!Encoding.UTF8.GetString(data).EndsWith("\r\n\r\n"))
            return;

        HttpRequest  request  = HttpRequest.ParseHttpRequest(this._data.ToArray());
        HttpResponse response = new();

        //Makes sure they dont traverse the dirtree upward
        if (request.RequestUri.Contains("..")) {
            response.StatusCode   = 403;
            response.ReasonPhrase = "IM DEAD";
            response.MessageBody  = Encoding.UTF8.GetBytes("<b>You bitch ass motherfucker you duped moneybags didn't you?</b>");
            this.SendResponse(response);
            return;
        }

        string path = Path.GetFullPath(Path.Combine(Server.ExecutablePath, Server.DATA_FOLDER, request.RequestUri.TrimStart('/').TrimStart('\\')));

        if (Directory.Exists(path))
            path = Path.Combine(path, "index.html");

        //Checks if the file exists
        if (!File.Exists(path)) {
            response.StatusCode   = 404;
            response.ReasonPhrase = "What the fuck is this?";
            response.MessageBody  = Encoding.UTF8.GetBytes("<b>where the hell am i?</b>");
            this.SendResponse(response);
            return;
        }

        //Read the target file and set the response body
        response.MessageBody = File.ReadAllBytes(path);
        //Set the content type
        response.ContentType = Path.GetExtension(path).ToLower() switch {
            ".html" => "text/html; charset=UTF-8",
            ".htm"  => "text/html; charset=UTF-8",
            ".ico"  => "image/x-icon",
            ".png"  => "image/png",
            ".jpg"  => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif"  => "image/gif",
            ".txt"  => "text/plain",
            ".css"  => "text/css",
            ".xml"  => "text/xml",
            ".js"   => "application/javascript",
            ".zip"  => "application/zip",
            _       => "application/octet-stream"
        };

        this.SendResponse(response);
    }

    public void SendResponse(HttpResponse response) {
        this.Stream.Write((byte[])response);
        this.Stream.Flush();
        this.Client.Close();
    }

    protected override void HandleDisconnect() {}
}
