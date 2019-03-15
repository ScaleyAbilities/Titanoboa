using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Titanoboa
{
    public static class CobraHelper
    {
        private static string QuoteSrvEndpoint = "https://localhost:5001/quote";
        
        public static decimal GetQuote(User user, string stockSymbol)
        {
            decimal amount;
            string cryptokey;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(QuoteSrvEndpoint + "/" + user.Username + "/" + stockSymbol + "");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            
            string[] recv = responseString.Split(",");
            amount = decimal.Parse(recv[0]);
            cryptokey = recv[1];

            var logger = new Logger();
            logger.LogQuoteServer(user, amount, stockSymbol, user.Username, DateTime.Now.ToString(), cryptokey);
            logger.CommitLogs();

            return amount;
        }
    }
}
