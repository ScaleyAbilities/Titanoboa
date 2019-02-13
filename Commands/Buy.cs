using System;
using System.Data;
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

            // Log the command
            Program.Logger.LogCommand(user, amount, stockSymbol);
            
            if (user.PendingBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            // Get current stock price
            var stockPrice = TransactionHelper.GetStockPrice(user, stockSymbol);

            if (amount < stockPrice)
            {
                throw new InvalidOperationException("Not enough money for single stock purchase.");
            }

            var stockAmount = (int)(amount / stockPrice);
            var balanceChange = stockAmount * stockPrice * -1;

            var transaction = TransactionHelper.CreateTransaction(user, stockSymbol, "BUY", balanceChange, stockAmount, stockPrice, "pending");
        } 
    }
}
