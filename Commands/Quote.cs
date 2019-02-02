using System;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        /*
            Quote command flow:
            1- Hit quote server for most up-to-date price
            2- Log event in Logs table
         */
        public static void Quote(string username, JObject commandParams) 
        {
            ParamHelper.ValidateParamsExist(commandParams, "stock");
            
            var stockSymbol = (string)commandParams["stock"];
            var user = TransactionHelper.GetUser(username);

            Program.Logger.LogCommand("QUOTE", user, null, stockSymbol);

            var stockPrice = TransactionHelper.GetStockPrice(user, stockSymbol);

            // TODO: Do something with price
        }
    }
}