using System;

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
        public void CommitBuy() {
            var user = databaseHelper.GetUser(username, false);

            logger.LogCommand(user);
            
            var transaction = databaseHelper.GetLatestPendingTransaction(user, "BUY");
            if (transaction == null)
            {
                throw new InvalidOperationException("No pending BUY transactions to commit.");
            }

            var newBalance = user.Balance - Math.Abs(transaction.BalanceChange);
            databaseHelper.UpdateUserBalance(ref user, newBalance);

            var stockName = transaction.StockSymbol;
            var stockAmount = transaction.StockAmount;

            var userStockAmount = databaseHelper.GetStocks(user, stockName);
            var newStockAmount = (stockAmount ?? 0) + userStockAmount;

            databaseHelper.UpdateStocks(user, stockName, newStockAmount);
            databaseHelper.CommitTransaction(ref transaction);
        }
    }
}