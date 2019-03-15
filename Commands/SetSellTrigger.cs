using System;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        /*
            Set Sell Trigger command flow:
            1- Get current user
            2- Find the stock trigger entry in transaction table
            3- Calculate stock amount to be sold (only whole stocks, based on user amount and stock price)
            4- Update number of stocks in transactions table
         */
        public async Task SetSellTrigger() 
        {
            // Sanity check
            CheckParams("price", "stock");

            // Get params
            var user = await databaseHelper.GetUser(username, true);
            var sellPrice = (decimal)commandParams["price"];
            var stockSymbol = commandParams["stock"].ToString();

            // Log command
            logger.LogCommand(user, sellPrice, stockSymbol);
            
            // Can't have a sell price of 0
            if (sellPrice == 0)
            {
                throw new InvalidOperationException("Can't have a sell price of 0.");
            }

            // Get the existing trigger transaction to be set
            var existingSellTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSellTrigger == null)
            {
                throw new InvalidOperationException("Can't set SELL_TRIGGER: No existing trigger");
            }

            // Make sure the trigger stock price or stock amount hasn't already been set
            if (existingSellTrigger.StockPrice != null || existingSellTrigger.StockAmount != null)
            {
                throw new InvalidOperationException("Can't set SELL_TRIGGER: Trigger was already set!");
            }

            // Find amount in $$ the user wants to sell of their stock
            var sellAmountInDollars = existingSellTrigger.BalanceChange;
            if (sellAmountInDollars <= 0)
            {
                throw new InvalidOperationException("Can't set SELL_TRIGGER: Trigger dollars amount was never set!");
            }

            // Make sure the price isn't higher than the amount they want to sell
            if (sellAmountInDollars < sellPrice)
            {
                throw new InvalidOperationException("Can't sell less than 1 stock.");
            }

            // Get the users pending stocks, error if not enough
            var userStockAmountPending = await databaseHelper.GetStocks(user, stockSymbol, true);
            if (userStockAmountPending <= 0)
            {
                throw new InvalidOperationException("User doesn't have enough stock to sell!.");
            }

            // Calculate whole num of stocks to be sold
            var numStockToSell = (int)Math.Floor(sellAmountInDollars / sellPrice);

            // Just sell all available stocks if they don't have enough
            numStockToSell = (numStockToSell > userStockAmountPending) ? userStockAmountPending : numStockToSell;

            // Set transaction StockAmount and StockPrice
            await databaseHelper.SetTransactionNumStocks(existingSellTrigger, numStockToSell);
            await databaseHelper.SetTransactionStockPrice(existingSellTrigger, sellPrice);

            // Send new trigger to Twig
            JObject twigTrigger = new JObject();
            JObject twigParams = new JObject();
            twigTrigger.Add("usr", username);
            twigTrigger.Add("cmd", "SELL");
            twigParams.Add("stock", existingSellTrigger.StockSymbol);
            twigParams.Add("price", sellPrice);
            twigTrigger.Add("params", twigParams);
            RabbitHelper.PushTrigger(twigTrigger);
        }
    }
}
