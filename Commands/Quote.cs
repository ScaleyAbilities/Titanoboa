using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        /*
            Quote command flow:
            1- Hit quote server for most up-to-date price
            2- Log event in Logs table
         */
        public async Task Quote() 
        {
            CheckParams("stock");
            
            var stockSymbol = (string)commandParams["stock"];
            var user = await databaseHelper.GetUser(username);

            logger.LogCommand(user, command, null, stockSymbol);

            // If we aren't returning anything then return here
            if (string.IsNullOrEmpty(returnRef))
                return;

            var stockPrice = await databaseHelper.GetStockPrice(user, stockSymbol);
            var ret = new JObject();
            ret.Add(stockSymbol, stockPrice);

            returnValue = ret;
        }
    }
}