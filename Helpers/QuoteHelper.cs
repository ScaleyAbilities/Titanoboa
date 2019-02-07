using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class QuoteHelper
    {
        // Quote server port number
        private const int quotePort = 4448;
        // Quote server url
        private static string quoteServer = "quoteserve.seng.uvic.ca";
        private static Socket skt;
        static QuoteHelper() {
            //https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(quoteServer);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, quotePort);

                skt = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                skt.Connect(remoteEP);
                Console.WriteLine("Socket connected to {0}",  skt.RemoteEndPoint.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to establish connection with Quote Server: {0}", e.ToString());
            }
        }

        public static decimal GetQuote(User user, string stockSymbol){
            
            var bytes = new byte[1024];
            var msg = Encoding.ASCII.GetBytes(user + "," + stockSymbol);
            
            var bytesSent = skt.Send(msg);
            var bytesRecv = skt.Receive(bytes);
            
            var msgRecv = Encoding.UTF8.GetString(bytes);
            var recv = msgRecv.Split(',');

            var amount = decimal.Parse(recv[0]);
            var timestamp = UnixTimeStampToDateTime(double.Parse(recv[3]) / 1000);
            var cryptokey = recv[4];
            Program.Logger.LogQuoteServer(user, amount, stockSymbol, timestamp, cryptokey);
            return amount;
        }

        private static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
            return dtDateTime;
        }
    }
}