using System;
using System.Text;
using System.Collections.Generic;

namespace pTyping.Web {
    public class HttpResponse {
        public static string LineTerminator = "\r\n";

        public static string HttpVersion  = "HTTP/1.1";
        public        short  StatusCode = 200;
        public        string ReasonPhrase = "OK";

        public Dictionary<string, string> Headers = new();

        public string MessageBody = "<b>Hewwowo worwd!</b><br><br><title>daddy uwu</title>";

        public static string DatePattern = "{0:ddd,' 'dd' 'MMM' 'yyyy' 'HH':'mm':'ss' 'K}";
        
        public HttpResponse() {
            this.Headers.Add("date",   string.Format(DatePattern, DateTime.UtcNow));
            this.Headers.Add("server", HttpServer.Server);
            this.Headers.Add("content-type", "text/html; charset=UTF-8");
        }

        private void UpdateContentLengthHeader() {
            this.Headers.Remove("content-length");
            this.Headers.Add("content-length", Encoding.UTF8.GetByteCount(this.MessageBody).ToString());
        }
        
        public override string ToString() {
            string finalString = $"{HttpVersion} {this.StatusCode} {this.ReasonPhrase}{LineTerminator}";
            this.UpdateContentLengthHeader();
            foreach ((string header, string data) in this.Headers) {
                finalString += $"{header}: {data}{LineTerminator}";
            }
            finalString += LineTerminator;
            finalString += this.MessageBody;

            return finalString;
        }

        public static implicit operator byte[](HttpResponse response) {
            return Encoding.UTF8.GetBytes(response.ToString());
        }
    }
}
