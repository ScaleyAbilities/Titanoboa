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
            Set Buy Trigger command flow:
            1- Get current user
            2- Find the stock trigger entry in transaction table
            3- Calculate stock amount (only whole stocks, based on user spending and stock price)
            4- Update spending balance, and number of stocks in transactions table
         */
        public static void SetBuyTrigger(string username, JObject commandParams)
        {
            // Sanity check
            ParamHelper.ValidateParamsExist(commandParams, "price", "stock");

            // Get params
            var user = TransactionHelper.GetUser(username, true);
            var buyPrice = (decimal)commandParams["price"];
            var stockSymbol = commandParams["stock"].ToString();

            // Log command
            Program.Logger.LogCommand(user, buyPrice, stockSymbol);

            // Can't have buy price of 0
            if (buyPrice == 0)
            {
                throw new InvalidOperationException("Can't have a buy price of 0.");
            }
            
            // Make sure trigger was previously created
            var existingBuyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingBuyTrigger == null)
            {
                throw new InvalidOperationException("Can't set BUY_TRIGGER: No existing trigger");
            }
            
            // Make sure the trigger hasn't already been set
            if (existingBuyTrigger.StockPrice != null || existingBuyTrigger.StockAmount != null)
            {
                throw new InvalidOperationException("Can't set BUY_TRIGGER: Trigger was already set!");
            }

            // Find amount in $$ the user wants to buy of the stock
            var buyAmountInDollars = existingBuyTrigger.BalanceChange;
            if (buyAmountInDollars <= 0)
            {
                throw new InvalidOperationException("Can't set BUY_TRIGGER: Trigger dollars amount was never set!");
            }

            // Update the transaction price
            TransactionHelper.SetTransactionStockPrice(ref existingBuyTrigger, buyPrice);

            // Send new trigger to Twig
            JObject twigTrigger = new JObject();
            twigTrigger["User"] = existingBuyTrigger.User.Id;
            twigTrigger["Command"] = "BUY";
            twigTrigger["StockSymbol"] = existingBuyTrigger.StockSymbol;
            twigTrigger["StockPrice"] = buyPrice;
            RabbitHelper.PushCommand(twigTrigger);
        }
    }
}
