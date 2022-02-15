using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{

    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        NotFound = 404,
        BadRequest = 400,
        Redirect = 301
    }

    class Response
    {
        string responseString;
        public string ResponseString
        {
            get
            {
                return responseString;
            }
        }
        StatusCode code;
        List<string> headerLines = new List<string>();
       // from server --> handle request
        public Response(StatusCode code, string contentType, string content, string redirectoinPath)
        {
            //throw new NotImplementedException();
            // TODO: Add headlines (Content-Type, Content-Length,Date, [location if there is redirection])

            this.code = code;
            string status_line = GetStatusLine(code);
            headerLines.Add("Content-Type: " + contentType + "\r\n");
            headerLines.Add("Content-Length: " + content.Length + "\r\n");
            headerLines.Add("Date Time : " + DateTime.Now + "\r\n");

            // TODO: Create the request string

            if (code == StatusCode.Redirect)
            {
                headerLines.Add("Location: " + redirectoinPath + "\r\n");
            }

            responseString = status_line;
            int i = 0;
            while (i < headerLines.Count)
            {
                responseString += headerLines[i];
                i++;
            }
            responseString += "\r\n";
            responseString += content;     

        }

        private string GetStatusLine(StatusCode code)
        {

            // TODO: Create the response status line and return it
            string statusLine = "HTTP/1.1 " + ((int)code).ToString() + " " + code.ToString() + "\r\n";
            return statusLine;
        }
    }
}