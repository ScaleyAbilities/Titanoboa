using System;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        public async Task CommitSellTrigger()
        {
            // Sanity check
            CheckParams("price", "stock");

            // Get params
            var user = await databaseHelper.GetUser(username);
            var committedSellPrice = (decimal)commandParams["price"];
            var stockSymbol = commandParams["stock"].ToString();

            // Log command
            logger.LogCommand(user, command, committedSellPrice, stockSymbol);

            // Can't have sell price of 0
            if (committedSellPrice <= 0)
            {
                throw new InvalidOperationException("Can't have a sell price of 0.");
            }

            // Get the existing trigger transaction to be committed
            var existingSellTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSellTrigger == null)
            {
                throw new InvalidOperationException("Can't commit SELL_TRIGGER: Trigger may have been cancelled!");
            }

            // Make sure that the max amount of stocks to be sold was set
            var numStocksToSell = existingSellTrigger.StockAmount ?? 0;
            if (numStocksToSell <= 0)
            {
                throw new InvalidOperationException("Can't commit sell of less than 1 stock.");
            }

            // Double check that the trigger worked properly
            var minSellPrice = existingSellTrigger.StockPrice;
            if(minSellPrice > committedSellPrice)
            {
                throw new InvalidProgramException("Program Error! Trigger sold for less than the min price");
            }

            // Calculate + update new user balance
            var moneyMade = committedSellPrice * numStocksToSell;
            var newUserBalance = user.Balance + moneyMade;
            await databaseHelper.UpdateUserBalance(user, newUserBalance);

            var userStockAmount = await databaseHelper.GetStocks(user, stockSymbol);
            var newStockAmount = userStockAmount - numStocksToSell;
            await databaseHelper.UpdateStocks(user, stockSymbol, newStockAmount);
    
            // Set transaction StockAmount and StockPrice, mark as completed
            await databaseHelper.SetTransactionStockPrice(existingSellTrigger, committedSellPrice);
            await databaseHelper.SetTransactionBalanceChange(existingSellTrigger, moneyMade); 
            await databaseHelper.CommitTransaction(existingSellTrigger);      
        }
    }
}