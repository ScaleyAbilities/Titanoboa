using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        /*
            CommitSell command flow:
            1- Get most recent Sell (within 60 seconds), 
            2- Add funds
            3- Remove stock amounts
            3- Update buy in transactions table, *set pending flag to false, and update timestamp*
         */
        public async Task CommitSell()
        {
            var user = await databaseHelper.GetUser(username, false);
            
            logger.LogCommand(user);

            var transaction = await databaseHelper.GetLatestPendingTransaction(user, "SELL");
            if (transaction == null)
            {
                throw new InvalidOperationException("No pending SELL transactions to commit.");
            }

            var newBalance = user.Balance + Math.Abs(transaction.BalanceChange);
            await databaseHelper.UpdateUserBalance(user, newBalance);

            var stockName = transaction.StockSymbol;
            var stockAmount = transaction.StockAmount ?? 0;
   
            var userStockAmount = await databaseHelper.GetStocks(user, stockName);
            var newStockAmount = userStockAmount - stockAmount;

            await databaseHelper.UpdateStocks(user, stockName, newStockAmount);
            await databaseHelper.CommitTransaction(transaction);
        }
    }
}