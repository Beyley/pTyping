using System;
using System.Text;
using System.Collections.Generic;

namespace pTyping.Web {
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

        public HttpMethod Method;
        public string RequestUri;
        public string HttpVersion;

        public Dictionary<string, string> Headers = new();

        public static HttpRequest ParseHttpRequest(byte[] data) {
            if (data.Length == 0) {
                return null;
            }
            
            HttpRequest request = new();

            string requestString = Encoding.UTF8.GetString(data);
            string[] splitRequest = requestString.Split("\r\n");

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
}
