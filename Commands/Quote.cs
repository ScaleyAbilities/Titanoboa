using System;
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
        public void Quote() 
        {
            CheckParams("stock");
            
            var stockSymbol = (string)commandParams["stock"];
            var user = databaseHelper.GetUser(username);

            logger.LogCommand(user, null, stockSymbol);

            var stockPrice = databaseHelper.GetStockPrice(user, stockSymbol);

            // TODO: Do something with price
        }
    }
}