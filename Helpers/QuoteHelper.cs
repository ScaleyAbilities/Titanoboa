using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

        public static decimal GetQuote(User user, string stockSymbol) {
            if(!usingQuoteSrv)
                return 10.00m;

            var ipHostInfo = Dns.GetHostEntry(quoteServer);
            var ipAddress = ipHostInfo.AddressList[0];
            var remoteEndPoint = new IPEndPoint(ipAddress, quotePort);
            
            Tuple<decimal, DateTime> cachedQuote = null;
            quoteCache.TryGetValue(stockSymbol, out cachedQuote);
            
            if (cachedQuote == null) {
                Console.WriteLine($"Quote Cache Miss: {stockSymbol}");
                var amount = GetQuoteFromQuoteServer(user, stockSymbol, ipHostInfo, ipAddress, remoteEndPoint);
                return amount;
            }

            if (cachedQuote.Item2.AddMinutes(1) <= DateTime.Now) {
                Thread thread = null;
                thread = new Thread(() => {
                    GetQuoteFromQuoteServer(user, stockSymbol, ipHostInfo, ipAddress, remoteEndPoint);
                });

                quoteCache[stockSymbol] = new Tuple<decimal, DateTime>(quoteCache[stockSymbol].Item1, DateTime.Now);
                thread.Start();
            }

            return cachedQuote.Item1;
        }
        
        private static decimal GetQuoteFromQuoteServer(User user, string stockSymbol, IPHostEntry ipHostInfo, IPAddress ipAddress, IPEndPoint remoteEndPoint)
        {
            decimal amount;
            
            using (var skt = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                skt.Connect(remoteEndPoint);
                Console.WriteLine("Socket connected to {0}",  skt.RemoteEndPoint.ToString());

                var bytes = new byte[1024];
                var msg = Encoding.ASCII.GetBytes($"{stockSymbol},{user.Username}\n");

                var bytesSent = skt.Send(msg);
                var bytesRecv = skt.Receive(bytes);

                var msgRecv = Encoding.UTF8.GetString(bytes).Replace("\0", string.Empty).Trim();
                var recv = msgRecv.Split(',');

                Console.WriteLine($"Quote Server Message: {msgRecv}");

                amount = decimal.Parse(recv[0]);
                var quoteStockSymbol = recv[1];
                var quoteUserId = recv[2];
                var timestamp = recv[3];
                var cryptokey = recv[4];

                var logger = new Logger("");
                logger.LogQuoteServer(user, amount, quoteStockSymbol, quoteUserId, timestamp, cryptokey);
                logger.CommitLogs();
            }

            quoteCache[stockSymbol] = new Tuple<decimal, DateTime>(amount, DateTime.Now);
            return amount;
        }
    }
}
