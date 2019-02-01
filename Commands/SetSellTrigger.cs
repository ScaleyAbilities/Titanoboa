using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        /*
            Set Sell Trigger command flow:
            1- Get current user
            2- Find the stock trigger entry in transaction table
            3- Calculate stock amount to be sold (only whole stocks, based on user amount and stock price)
            3- Update number of stocks in transactions table
         */
        public static void SetSellTrigger(string username, JObject commandParams) 
        {
            ParamHelper.ValidateParamsExist(commandParams, "stockPrice", "stock");

            // Unpack JObject
            var sellPrice = (decimal)commandParams["stockPrice"];
            var stockSymbol = commandParams["stock"].ToString();

            // Get users current balance
            var user = TransactionHelper.GetUser(username, true);

            TransactionHelper.UpdateTriggerTransaction(user, stockSymbol, "SELLTRIGGER", sellPrice);
        } 
    }
}
