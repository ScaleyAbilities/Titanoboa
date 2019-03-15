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
            3- Update number of stocks in transactions table
         */
        public async Task SetSellTrigger() 
        {
            CheckParams("price", "stock");

            // Unpack JObject
            var sellPrice = (decimal)commandParams["price"];
            var stockSymbol = commandParams["stock"].ToString();

            // Get users current balance
            var user = await databaseHelper.GetUser(username, true);

            logger.LogCommand(user, sellPrice, stockSymbol);

            // Get the existing trigger to find amount in $$ the user wants to sell of their stock
            var existingTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
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
            var userStockAmount = await databaseHelper.GetStocks(user, stockSymbol, true);
            var numStockToSell = (int)Math.Floor(sellAmount / sellPrice);
            if (userStockAmount < numStockToSell)
            {
                throw new InvalidOperationException("Insufficient stock for SET_SELL_TRIGGER.");
            }

            // Subtract stocks from user account
            // any extra $$ will be refunded upon trigger point being hit / cancel trigger events
            var newUserStockAmount = userStockAmount - numStockToSell;

            await databaseHelper.UpdateStocks(user, stockSymbol, newUserStockAmount);
            await databaseHelper.SetTransactionStockPrice(existingTrigger, sellPrice);
        } 
    }
}
