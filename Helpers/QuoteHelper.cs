using System;
using System.Net;
using System.Net.Sockets;

namespace Titanoboa
{
    public static class QuoteHelper
    {
        // Quote server port number
        private const int quotePort = "4448";
        // Quote server url
        private static string quoteServer = "quoteserve.seng.uvic.ca";

        private bool connectionEstablished(){
            // Got the follow lines of code from microsoft .Net documentation. Might not be best?
            // https://docs.microsoft.com/en-us/dotnet/framework/network-programming/synchronous-client-socket-example
            IPHostEntry ipHostInfo = Dns.GetHostEntry(quoteServer);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, quotePort);

            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);
        }

        public static int Main(String[] args) {
            if(connectionEstablished()){
                return 0;
            }
            else{
                return 1;
            }
        }
    }
}