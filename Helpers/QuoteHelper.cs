using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class QuoteHelper
    {
        // Quote server port number
        private const int quotePort = 4448;
        // Quote server url
        private static string quoteServer = "quoteserve.seng.uvic.ca";

        public static Socket AsyncConnect() {
            //https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(quoteServer);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, quotePort);

                Socket skt = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                skt.Connect(remoteEP);
                Console.WriteLine("Socket connected to {0}",  skt.RemoteEndPoint.ToString());

                return skt;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to establish connection with Quote Server: {0}", e.ToString());
                return null;
            }
        }
    }
}