using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class Commands
    {
        /*
            Set Sell Trigger command flow:
            1- Get current user
            2- Find the stock trigger entry in transaction table
            3- Calculate stock amount to be sold (only whole stocks, based on user amount and stock price)
            3- Update number of stocks in transactions table
         */
        public void SetSellTrigger(string username, JObject commandParams) 
        {
            ParamHelper.ValidateParamsExist(commandParams, "price", "stock");

            // Unpack JObject
            var sellPrice = (decimal)commandParams["price"];
            var stockSymbol = commandParams["stock"].ToString();

            // Get users current balance
            var user = databaseHelper.GetUser(username, true);

            Program.Logger.LogCommand(user, sellPrice, stockSymbol);

            // Get the existing trigger to find amount in $$ the user wants to sell of their stock
            var existingTrigger = databaseHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingTrigger == null)
            {
                throw new InvalidOperationException("No existing trigger");
            }

            var sellAmount = existingTrigger.BalanceChange;

            if (sellAmount == 0)
            {
                throw new System.InvalidOperationException("Cannot sell 0 dollars worth of stocks.");
            }

            // Check if they have enough to sell
            var userStockAmount = databaseHelper.GetStocks(user, stockSymbol, true);
            var numStockToSell = (int)Math.Floor(sellAmount / sellPrice);
            if (userStockAmount < numStockToSell)
            {
                throw new InvalidOperationException("Insufficient stock for SET_SELL_TRIGGER.");
            }

            // Subtract stocks from user account
            // any extra $$ will be refunded upon trigger point being hit / cancel trigger events
            var newUserStockAmount = userStockAmount - numStockToSell;
            databaseHelper.UpdateStocks(user, stockSymbol, newUserStockAmount);

            databaseHelper.SetTransactionStockPrice(ref existingTrigger, sellPrice);
        } 
    }
}
