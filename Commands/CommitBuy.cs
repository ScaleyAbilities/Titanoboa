using System;
using System.Threading.Tasks;

namespace Titanoboa
{
    public partial class CommandHandler 
    {
        /*
            CommitBuy command flow:
            1- Get most recent buy (within 60 seconds), 
            2- Remove funds
            3- Add stock amounts
            3- Update buy in transactions table, *set pending flag to false, and update timestamp*
         */
        public async Task CommitBuy() {
            var user = await databaseHelper.GetUser(username, false);

            logger.LogCommand(user);
            
            var transaction = await databaseHelper.GetLatestPendingTransaction(user, "BUY");
            if (transaction == null)
            {
                throw new InvalidOperationException("No pending BUY transactions to commit.");
            }

            var newBalance = user.Balance - Math.Abs(transaction.BalanceChange);
            await databaseHelper.UpdateUserBalance(user, newBalance);

            var stockName = transaction.StockSymbol;
            var stockAmount = transaction.StockAmount;

            var userStockAmount = await databaseHelper.GetStocks(user, stockName);
            var newStockAmount = (stockAmount ?? 0) + userStockAmount;

            await databaseHelper.UpdateStocks(user, stockName, newStockAmount);
            await databaseHelper.CommitTransaction(transaction);
        }
    }
}