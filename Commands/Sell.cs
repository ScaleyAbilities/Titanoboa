using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        /*
         * - check quote server for current stock price
         * - check stock data table for amount of stocks the user owns
         * - calculate if user has enough to sell, stock amount * current price > selling price
         * - update transaction server
         */
        public void Sell()
        {
            CheckParams("amount", "stock");

            var sellAmount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();
            var user = databaseHelper.GetUser(username);

            logger.LogCommand(user, sellAmount, stockSymbol);

            // Get current stock price
            var stockPrice = databaseHelper.GetStockPrice(user, stockSymbol);

            // Get amount of stocks user owns
            var pendingStockAmount = databaseHelper.GetStocks(user, stockSymbol, true);

            // Check that user has enough stocks to sell.
            if (sellAmount > pendingStockAmount * stockPrice)
            {
                throw new InvalidOperationException(
                    $"Insufficient stocks ({stockSymbol}): ({pendingStockAmount}) at selling price ({sellAmount}), current stock price: {stockPrice}"
                );
            }

            var stockAmount = (int)Math.Floor(sellAmount / stockPrice);

            // Set balance change
            var balanceChange = stockAmount * stockPrice;

            // Set NEGATIVE stockAmount (to remove from stocks table in COMMIT_SELL)
            stockAmount = -stockAmount;

            var transaction = databaseHelper.CreateTransaction(user, stockSymbol, "SELL", balanceChange, stockAmount, stockPrice, "pending");
        } 
    }
}
