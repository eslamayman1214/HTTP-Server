using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPServer
{
    public enum RequestMethod
    {
        GET,
        POST,
        HEAD
    }

    public enum HTTPVersion
    {
        HTTP10,
        HTTP11,
        HTTP09
    }

    class Request
    {
        string[] requestLine;
        RequestMethod method;
        public string relativeURI;
        Dictionary<string, string> headerLines;

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }

        HTTPVersion httpVersion;
        //request parameter
        string requestString; //first line in http req -> get uri ver
        string[] contentLines;
        //request splited
        string[] Requestlines;


        public Request(string requestString)
        {
            this.requestString = requestString;


        }
        /// <summary>
        /// Parses the request string and loads the request line, header lines and content, returns false if there is a parsing error
        /// </summary>
        /// <returns>True if parsing succeeds, false otherwise.</returns>

        public bool ParseRequest()
        {
            //TODO: parse the receivedRequest using the \r\n delimeter   
            string[] Seperator = { "\r\n" };   //craft CRLF
            Requestlines = requestString.Split(Seperator, StringSplitOptions.None);

            // check that there is atleast 3 lines: Request line, Host Header, Blank line (usually 4 lines with the last empty line for empty content)
            if (Requestlines.Length < 3)
            {
                return false;
            }

            // Parse Request line method uri version
            requestLine = Requestlines[0].Split(' ');

            // Load header lines into HeaderLines dictionary
            // Validate blank line exists

            if (ParseRequestLine() == true && LoadHeaderLines() == true && ValidateBlankLine() == true)
                return true;
            else
                return false;


        }

        private bool ParseRequestLine()
        {
            if (requestLine.Length < 2)
            {
                return false;
            }

            else if (requestLine.Length == 2)
            {
                httpVersion = HTTPVersion.HTTP09;
            }

            else
            {
                if (requestLine[2] == "HTTP/1.0")
                    httpVersion = HTTPVersion.HTTP10;
                else if (requestLine[2] == "HTTP/1.1") 
                    httpVersion = HTTPVersion.HTTP11; 
                else 
                    return false; 

            }
            if (requestLine[0].ToLower() == "get")
                method = RequestMethod.GET;
            else if(requestLine[0].ToLower() == "post")
                method = RequestMethod.POST;
            else if(requestLine[0].ToLower() == "head")
                 method = RequestMethod.HEAD;
            else
                return false;          

            relativeURI = requestLine[1];
            bool validate = ValidateIsURI(relativeURI);
            return validate;
        }

        private bool ValidateIsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);
        }

        private bool LoadHeaderLines()
        {

            headerLines = new Dictionary<string, string>();
            int len_headerlines = Requestlines.Length - 3;
            for (int i = 1; i <= len_headerlines; i++)
            {
                if (Requestlines[i].Contains(":"))
                {
                    string[] split_headerline = { ": " };
                    string[] content_headerline = Requestlines[i].Split(split_headerline, StringSplitOptions.None);
                    headerLines.Add(content_headerline[0], content_headerline[1]);

                }
                else 
                    return false;
            }
            return true;
        }
        private bool ValidateBlankLine()
        {
            int len_request = Requestlines.Length - 2;
            if (Requestlines[len_request] == "")
                return true;
            else
                return false;
        }


    }
}
