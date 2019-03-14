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
            4- Update number of stocks in transactions table
         */
        public static void SetSellTrigger(string username, JObject commandParams)
        {
            ParamHelper.ValidateParamsExist(commandParams, "price", "stock");

            // Unpack JObject
            var sellPrice = (decimal)commandParams["price"];
            if (sellPrice == 0)
            {
                throw new InvalidOperationException("Can't have a sell price of 0.");
            }
            var stockSymbol = commandParams["stock"].ToString();

            // Get users current balance
            var user = TransactionHelper.GetUser(username, true);

            // Log command
            Program.Logger.LogCommand(user, sellPrice, stockSymbol);

            // Get the existing trigger to 
            var existingSellTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSellTrigger == null)
            {
                throw new InvalidOperationException("Can't set trigger: No existing trigger");
            }
            // Make sure the trigger stock price hasn't already been set
            else if (existingSellTrigger.StockPrice != null)
            {
                throw new InvalidOperationException("Can't set trigger: Trigger was already set!");
            }
            // Make sure the trigger's amount was set
            else if (existingSellTrigger.BalanceChange == 0)
            {
                throw new InvalidOperationException("Can't set trigger: Trigger dollars amount was never set!");
            }

            // Find amount in $$ the user wants to sell of their stock
            var sellAmountInDollars = existingSellTrigger.BalanceChange;
            if (sellAmountInDollars == 0)
            {
                throw new System.InvalidOperationException("Cannot sell 0 dollars worth of stocks.");
            }

            // Make sure the price isn't higher than the amount they want to sell
            if (sellAmountInDollars < sellPrice)
            {
                throw new InvalidOperationException("Can't sell less than 1 stock.");
            }

            // Send new trigger to Twig
            dynamic twigTrigger = new JObject();

            // Populate JSON Object
            twigTrigger.User = existingSellTrigger.User;
            twigTrigger.Command = "SELL";
            twigTrigger.StockSymbol = existingSellTrigger.StockSymbol;
            twigTrigger.StockPrice = sellPrice;

            // Push twigTrigger to Rabbit Q
            RabbitHelper.PushCommand(twigTrigger);

            // Calculate whole num of stocks to be sold
            var numStockToSell = (int)Math.Floor(sellAmountInDollars / sellPrice);
            var userStockAmount = TransactionHelper.GetStocks(user, stockSymbol, true);

            // Subtract stocks from user account
            // any extra $$ will be refunded upon trigger point being hit / cancel trigger events
            var newUserStockAmount = userStockAmount - numStockToSell;

            // Just sell all stocks if they don't have enough
            newUserStockAmount = (newUserStockAmount < 0) ? 0 : newUserStockAmount;

            // Update the user amonut of stock
            TransactionHelper.UpdateStocks(user, stockSymbol, newUserStockAmount);

            // Update the selling price
            TransactionHelper.SetTransactionStockPrice(ref existingSellTrigger, sellPrice);
        }
    }
}
