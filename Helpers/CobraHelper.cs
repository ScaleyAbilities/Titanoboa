using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Titanoboa
{
    public static class CobraHelper
    {
        private static string QuoteSrvEndpoint = "https://localhost:5001/quote";
        
        public static async Task<decimal> GetQuote(User user, string stockSymbol)
        {
            decimal amount;
            string cryptokey;

            var request = WebRequest.Create(QuoteSrvEndpoint + "/" + user.Username + "/" + stockSymbol + "");
            var response = await request.GetResponseAsync();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            
            JObject responseJson = JObject.Parse(responseString);

            amount = decimal.Parse(responseJson["amount"].ToString());
            cryptokey = responseJson["cryptokey"].ToString();

            var logger = new Logger();
            logger.LogQuoteServer(user, amount, stockSymbol, user.Username, DateTime.Now.ToString(), cryptokey);
            await logger.CommitLogs();

            return amount;
        }
    }
}
