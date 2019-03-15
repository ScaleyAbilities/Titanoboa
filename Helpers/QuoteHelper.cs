using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
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

        private static ConcurrentDictionary<string, Tuple<decimal, DateTime>> quoteCache = new ConcurrentDictionary<string, Tuple<decimal, DateTime>>();

        private static bool usingQuoteSrv = Environment.GetEnvironmentVariable("USING_QUOTE_SRV") == "TRUE" ? true : false;

        public static async Task<decimal> GetQuote(User user, string stockSymbol, Logger logger) {
            if(!usingQuoteSrv)
                return 10.00m;

            var ipHostInfo = Dns.GetHostEntry(quoteServer);
            var ipAddress = ipHostInfo.AddressList[0];
            var remoteEndPoint = new IPEndPoint(ipAddress, quotePort);
            
            Tuple<decimal, DateTime> cachedQuote = null;
            quoteCache.TryGetValue(stockSymbol, out cachedQuote);
            
            if (cachedQuote == null) {
                Console.WriteLine($"Quote Cache Miss: {stockSymbol}");
                var amount = await GetQuoteFromQuoteServer(user, stockSymbol, ipHostInfo, ipAddress, remoteEndPoint, logger);
                return amount;
            }

            if (cachedQuote.Item2.AddMinutes(1) <= DateTime.Now) {
                _ = GetQuoteFromQuoteServer(user, stockSymbol, ipHostInfo, ipAddress, remoteEndPoint, logger);
                quoteCache[stockSymbol] = new Tuple<decimal, DateTime>(quoteCache[stockSymbol].Item1, DateTime.Now);
            }

            return cachedQuote.Item1;
        }
        
        private static async Task<decimal> GetQuoteFromQuoteServer(User user, string stockSymbol, IPHostEntry ipHostInfo, IPAddress ipAddress, IPEndPoint remoteEndPoint, Logger logger)
        {
            decimal amount;
            
            using (var skt = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                await skt.ConnectAsync(remoteEndPoint);
                Console.WriteLine("Socket connected to {0}",  skt.RemoteEndPoint.ToString());

                var bytes = new byte[1024];
                var msg = Encoding.ASCII.GetBytes($"{stockSymbol},{user.Username}\n");

                var bytesSent = await SocketTaskExtensions.SendAsync(skt, msg, SocketFlags.None);
                var bytesRecv = await SocketTaskExtensions.ReceiveAsync(skt, bytes, SocketFlags.None);

                var msgRecv = Encoding.UTF8.GetString(bytes).Replace("\0", string.Empty).Trim();
                var recv = msgRecv.Split(',');

                Console.WriteLine($"Quote Server Message: {msgRecv}");

                amount = decimal.Parse(recv[0]);
                var quoteStockSymbol = recv[1];
                var quoteUserId = recv[2];
                var timestamp = recv[3];
                var cryptokey = recv[4];

                logger.LogQuoteServer(user, amount, quoteStockSymbol, quoteUserId, timestamp, cryptokey);
            }

            quoteCache[stockSymbol] = new Tuple<decimal, DateTime>(amount, DateTime.Now);
            return amount;
        }
    }
}
