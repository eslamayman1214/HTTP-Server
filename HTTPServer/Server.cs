using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;

        public Server(int portNumber, string redirectionMatrixPath)
        {
            //TODO: call this.LoadRedirectionRules passing redirectionMatrixPath to it
            this.LoadRedirectionRules(redirectionMatrixPath);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, portNumber);
            //TODO: initialize this.serverSocket
            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
        }

        public void StartServer()
        {
            Console.WriteLine("Start Listening : ");
            // TODO: Listen to connections, with large backlog.
            serverSocket.Listen(500);
            // TODO: Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                //TODO: accept connections and start thread for each accepted connection.
                Socket clientSocket = serverSocket.Accept();
                Thread newthread = new Thread(new ParameterizedThreadStart(HandleConnection));
                newthread.Start(clientSocket);
            }
        }
        // program ->
        public void HandleConnection(object obj)
        {
            // for each recieved req , server must reply with response 
            // HandeleReq : take req and send response 
            // TODO: Create client socket 
            Socket clientSocket = (Socket)obj;
            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            clientSocket.ReceiveTimeout = 0;
            // TODO: receive requests in while true until remote client closes the socket.
             
            Console.WriteLine("Recieve Req From Client : " + clientSocket.RemoteEndPoint);
              
            while (true)
            {
                 try
                {
                    // TODO: Receive request
                    byte[] recievedata = new byte[1024 * 1024];
                    int receivedLength = clientSocket.Receive(recievedata);
                    string data = Encoding.ASCII.GetString(recievedata);
                    // TODO: break the while loop if receivedLen==0
                    if (receivedLength == 0)
                    {
                        Console.WriteLine("Client : " + clientSocket.RemoteEndPoint + " End connection");
                        break;
                    }

                    // TODO: Create a Request object using received request string
                    Request clientrequest = new Request(data);     // Request take requeststring as a parameter
                    // TODO: Call HandleRequest Method that returns the response
                    Response serverresponse = HandleRequest(clientrequest);
                    // TODO: Send Response back to client
                    byte[] response = Encoding.ASCII.GetBytes(serverresponse.ResponseString);
                    clientSocket.Send(response);

                }
                catch (Exception ex)
                {
                    // TODO: log exception using Logger class
                   // Logger log = new LogException(ex);
                    Logger.LogException(ex);
                }
            }

            // TODO: close client socket
            clientSocket.Close();
        }

        Response HandleRequest(Request request)
        {
            
            string content;
            try
            {
                 //throw new Exception();      // to add internal Server error 500
                //TODO: check for bad request 
                if(!request.ParseRequest())      // go to class request
                {
                    content = LoadDefaultPage(Configuration.BadRequestDefaultPageName);
                    return new Response(StatusCode.BadRequest, "html", content, "");
                }

                //TODO: map the relativeURI in request to get the physical path of the resource.
                string Path = Configuration.RootPath + request.relativeURI;

                //TODO: check for redirect
                string redirection_Path = GetRedirectionPagePathIFExist(request.relativeURI);
                if (redirection_Path!="")
                {
                    content = LoadDefaultPage(Configuration.RedirectionDefaultPageName);
                    Path = Configuration.RootPath + "/" + redirection_Path;
                    content = File.ReadAllText(Path);  //path -> physical path
                    string location = redirection_Path;
                    return new Response(StatusCode.Redirect, "html", content, location);
                }

                //TODO: check file exists 404
                if (!File.Exists(Path))
                {
                    content = LoadDefaultPage(Configuration.NotFoundDefaultPageName);
                    return new Response(StatusCode.NotFound, "html", content, "");
                }
                //TODO: read the physical file
                content = File.ReadAllText(Path);      // read what really given path
                // Create OK response
                return new Response(StatusCode.OK, "html", content, "");
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                // TODO: in case of exception, return Internal Server Error. 
                //server erorr 500
                content = LoadDefaultPage(Configuration.InternalErrorDefaultPageName);
                return new Response(StatusCode.InternalServerError, "html", content, "");
            }
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
            string RedirectionPath;
            if (relativePath[0] == '/') // -> /path
            {
                relativePath = relativePath.Substring(1);
            }

            bool exist = Configuration.RedirectionRules.TryGetValue(relativePath, out RedirectionPath);

            if (exist== false)
            {
                return "";
            }
            return RedirectionPath;
            
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            //path
            string ret_cont = "";   // return content
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            // TODO: check if filepath not exist log exception using Logger class and return empty string
           

            if (!File.Exists(filePath))
            {
                Logger.LogException(new Exception(defaultPageName + " Page not Exist"));
                return "";
            }

            // else read file and return its content
            ret_cont = File.ReadAllText(filePath);
            return ret_cont;
        }

        //handle redirection
        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                
                // TODO: using the filepath paramter read the redirection rules from file 
                string[] RulesLines = File.ReadAllLines(filePath);     // instead of open file then call streamReader to read file path 
                 Configuration.RedirectionRules = new Dictionary<string, string>();
                // then fill Configuration.RedirectionRules dictionary 
                int len = RulesLines.Length;
                // if length = 0 it won't enter the for loop
                for (int i = 0; i < len; i++)
                {
                    string[] Rule_data = RulesLines[i].Split(',');
                    // load rules in redirection rule dictionary
                    Configuration.RedirectionRules.Add(Rule_data[0], Rule_data[1]);         // Ruledata[0]-> key from client , Ruledata[1]-> value redirect on it

                }
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }
    }
}
