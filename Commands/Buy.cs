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
            1- Get current user balance
            2- Get current stock price
            3- Calculate stock amount (only whole stocks)
            3- Insert buy into transactions table, *set pending flag to true*
         */
        public static void Buy(string username, JObject commandParams) 
        {
            ParamHelper.ValidateParamsExist(commandParams, "amount", "stock");

            // Unpack JObject
            var amount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();

            // Get users current balance
            var user = TransactionHelper.GetUser(username, true);
            
            if (user.PendingBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            // Get current stock price
            var stockPrice = TransactionHelper.GetStockPrice(stockSymbol);
            if (amount < stockPrice)
            {
                throw new InvalidOperationException("Not enough money for stock purchase.");
            }

            var stockAmount = (int)(amount / stockPrice);
            var balanceChange = stockAmount * stockPrice * -1;

            TransactionHelper.AddTransaction(user, stockSymbol, "BUY", balanceChange, stockAmount, true);
        } 
    }
}
