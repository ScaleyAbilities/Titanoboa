using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class CobraHelper
    {
        private static string quoteEndpoint = "https://localhost:5001/api/quote";
        private const int quotePort = 5001;
        
        public static decimal GetQuote(User user, string stockSymbol)
        {
            var ipHostInfo = Dns.GetHostEntry(quoteEndpoint + "/" + user.Username + "/" + stockSymbol);
            var ipAddress = ipHostInfo.AddressList[0];
            var remoteEndPoint = new IPEndPoint(ipAddress, quotePort);

            decimal amount;
            string timestamp;
            string cryptokey;
            
            using (var skt = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                skt.Connect(remoteEndPoint);
                Console.WriteLine("Socket connected to {0}",  skt.RemoteEndPoint.ToString());

                var bytes = new byte[1024];

                var bytesSent = skt.Send(bytes);
                var bytesRecv = skt.Receive(bytes);

                var msgRecv = Encoding.UTF8.GetString(bytes).Replace("\0", string.Empty).Trim();
                var recv = msgRecv.Split(',');

                Console.WriteLine($"Quote Server Message: {msgRecv}");

                amount = decimal.Parse(recv[0]);
                cryptokey = recv[1];

                var logger = new Logger();
                logger.LogQuoteServer(user, amount, stockSymbol, user.Username, DateTime.Now.ToString(), cryptokey);
                logger.CommitLogs();
            }

            return amount;
        }
    }
}
