using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
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

        private static Dictionary<string, Tuple<decimal, DateTime>> quoteCache = new Dictionary<string, Tuple<decimal, DateTime>>();

        private static bool usingQuoteSrv = Environment.GetEnvironmentVariable("USING_QUOTE_SRV") == "TRUE" ? true : false;

        public static decimal GetQuote(User user, string stockSymbol){
            if(!usingQuoteSrv)
                return 10.00m;

            Tuple<decimal, DateTime> cachedQuote = null;
            quoteCache.TryGetValue(stockSymbol, out cachedQuote);

            if (cachedQuote != null && cachedQuote.Item2.AddMinutes(1) >= DateTime.Now)
                return cachedQuote.Item1;

            var ipHostInfo = Dns.GetHostEntry(quoteServer);
            var ipAddress = ipHostInfo.AddressList[0];
            var RemoteEndPoint = new IPEndPoint(ipAddress, quotePort);

            decimal amount;

            using (var skt = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                skt.Connect(RemoteEndPoint);
                Console.WriteLine("Socket connected to {0}",  skt.RemoteEndPoint.ToString());

                var bytes = new byte[1024];
                var msg = Encoding.ASCII.GetBytes($"{user},{stockSymbol}\n");

                var bytesSent = skt.Send(msg);
                var bytesRecv = skt.Receive(bytes);

                var msgRecv = Encoding.UTF8.GetString(bytes).Replace("\0", string.Empty).Trim();
                var recv = msgRecv.Split(',');

                amount = decimal.Parse(recv[0]);
                var timestamp = recv[3];
                var cryptokey = recv[4];

                Program.Logger.LogQuoteServer(user, amount, stockSymbol, timestamp, cryptokey);
            }

            quoteCache[stockSymbol] = new Tuple<decimal, DateTime>(amount, DateTime.Now);
            
            return amount;
        }
    }
}