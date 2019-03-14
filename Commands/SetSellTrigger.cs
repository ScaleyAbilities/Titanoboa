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
            ParamHelper.ValidateParamsExist(commandParams, "price", "stock");

            // Unpack JObject
            var sellPrice = (decimal)commandParams["price"];
            var stockSymbol = commandParams["stock"].ToString();

            // Get users current balance
            var user = TransactionHelper.GetUser(username, true);

            Program.Logger.LogCommand(user, sellPrice, stockSymbol);

            // Get the existing trigger to find amount in $$ the user wants to sell of their stock
            var existingSellTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSellTrigger == null)
            {
                throw new InvalidOperationException("No existing trigger");
            }            
            // Make sure the trigger hasn't already been set
            else if(existingSellTrigger.StockPrice == null) 
            {
                throw new InvalidOperationException("Can't set trigger: Trigger was already set!");
            } 
            // Make sure the trigger's amount was set
            else if(existingSellTrigger.StockAmount == null)
            {
                throw new InvalidOperationException("Can't set trigger: Trigger amount was never set!");
            }

            var sellAmount = existingSellTrigger.BalanceChange;

            if (sellAmount == 0)
            {
                throw new System.InvalidOperationException("Cannot sell 0 dollars worth of stocks.");
            }

            // Check if they have enough to sell
            var userStockAmount = TransactionHelper.GetStocks(user, stockSymbol, true);
            var numStockToSell = (int)Math.Floor(sellAmount / sellPrice);
            if (userStockAmount < numStockToSell)
            {
                throw new InvalidOperationException("Insufficient stock for SET_SELL_TRIGGER.");
            }

            // Subtract stocks from user account
            // any extra $$ will be refunded upon trigger point being hit / cancel trigger events
            var newUserStockAmount = userStockAmount - numStockToSell;
            TransactionHelper.UpdateStocks(user, stockSymbol, newUserStockAmount);

            TransactionHelper.SetTransactionStockPrice(ref existingSellTrigger, sellPrice);

            // Send new trigger to Twig
            dynamic twigTrigger = new JObject();

            // Populate JSON Object
            twigTrigger.Id = existingSellTrigger.Id;
            twigTrigger.User = existingSellTrigger.User;
            twigTrigger.Command = "SELL_TRIGGER";
            twigTrigger.StockSymbol = existingSellTrigger.StockSymbol;
            twigTrigger.StockAmount = existingSellTrigger.StockAmount;
            twigTrigger.StockPrice = sellPrice;

            // TODO: Push twigTrigger to Rabbit Q
        } 
    }
}
