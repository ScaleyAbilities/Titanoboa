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
            Buy command flow:
            1- Get current user
            2- Find the stock trigger entry in transaction table
            3- Calculate stock amount (only whole stocks, based on user spending and stock price)
            3- Update spending balance, and number of stocks in transactions table
         */
        public static void SetBuyTrigger(string username, JObject commandParams) 
        {
            ParamHelper.ValidateParamsExist(commandParams, "price", "stock");

            // Unpack JObject
            var buyPrice = (decimal)commandParams["price"];
            var stockSymbol = commandParams["stock"].ToString();

            // Get users current balance
            var user = TransactionHelper.GetUser(username, true);
            var buyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (buyTrigger == null)
            {
                throw new InvalidOperationException("No existing trigger");
            }

            TransactionHelper.SetTransactionStockPrice(ref buyTrigger, buyPrice);
            LogHelper.LogCommand(buyTrigger);
        } 
    }
}
