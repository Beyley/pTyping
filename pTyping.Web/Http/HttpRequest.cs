using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace pTyping.Web.Http;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum HttpMethod {
    OPTIONS,
    GET,
    HEAD,
    POST,
    PUT,
    DELETE,
    TRACE,
    CONNECT
}

public class HttpRequest {
    public static string LineTerminator = "\r\n";

    public Dictionary<string, string> Headers = new();
    public string                     HttpVersion;

    public HttpMethod Method;
    public string     RequestUri;

    public static HttpRequest ParseHttpRequest(byte[] data) {
        if (data.Length == 0)
            return null;

        HttpRequest request = new();

        string   requestString = Encoding.UTF8.GetString(data);
        string[] splitRequest  = requestString.Split("\r\n");

        string[] firstLine = splitRequest[0].Split(" ");

        request.Method      = Enum.Parse<HttpMethod>(firstLine[0]);
        request.RequestUri  = firstLine[1];
        request.HttpVersion = firstLine[2];

        for (int i = 1; i < splitRequest.Length; i++) {
            string requestLine = splitRequest[i];
            if (requestLine.Trim().Length == 0) continue;

            string[] splitLine = requestLine.Split(": ", 2);

            request.Headers.Add(splitLine[0], splitLine[1]);
        }

        return request;
    }
}
