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
            3- Update spending balance, and number of stocks in transactions table
         */
        public static void SetBuyTrigger(string username, JObject commandParams) 
        {
            ParamHelper.ValidateParamsExist(commandParams, "price", "stock");

            // Unpack JObject
            var buyPrice = (decimal)commandParams["price"];
            if(buyPrice == 0) 
            {
                throw new InvalidOperationException("Can't have a buy price of 0.");
            }
            var stockSymbol = commandParams["stock"].ToString();

            // Get users current balance
            var user = TransactionHelper.GetUser(username, true);

            // Log command
            Program.Logger.LogCommand(user, buyPrice, stockSymbol);

            var buyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            
            // Make sure trigger was previously created
            if (buyTrigger == null)
            {
                throw new InvalidOperationException("Can't set trigger: No existing trigger");
            } 
            // Make sure the trigger hasn't already been set
            else if(buyTrigger.StockPrice == null) 
            {
                throw new InvalidOperationException("Can't set trigger: Trigger was already set!");
            } 
            // Make sure the trigger's amount was set
            else if(buyTrigger.StockAmount == null)
            {
                throw new InvalidOperationException("Can't set trigger: Trigger amount was never set!");
            }

            // Update the transaction price
            TransactionHelper.SetTransactionStockPrice(ref buyTrigger, buyPrice);

            // Send new trigger to Twig
            dynamic twigTrigger = new JObject();

            // Populate JSON Object
            twigTrigger.Id = buyTrigger.Id;
            twigTrigger.User = buyTrigger.User;
            twigTrigger.Command = "BUY_TRIGGER";
            twigTrigger.StockSymbol = buyTrigger.StockSymbol;
            twigTrigger.StockAmount = buyTrigger.StockAmount;
            twigTrigger.StockPrice = buyPrice;

            // TODO: Push twigTrigger to Rabbit Q

        } 
    }
}
