using System;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class Commands
    {
        /*
            CommitSell command flow:
            1- Get most recent Sell (within 60 seconds), 
            2- Add funds
            3- Remove stock amounts
            3- Update buy in transactions table, *set pending flag to false, and update timestamp*
         */
        public void CommitSell(string username)
        {
            var user = databaseHelper.GetUser(username, false);
            
            Program.Logger.LogCommand(user);

            var transaction = databaseHelper.GetLatestPendingTransaction(user, "SELL");
            if (transaction == null)
            {
                throw new InvalidOperationException("No pending SELL transactions to commit.");
            }

            var newBalance = user.Balance + Math.Abs(transaction.BalanceChange);
            databaseHelper.UpdateUserBalance(ref user, newBalance);

            var stockName = transaction.StockSymbol;
            var stockAmount = transaction.StockAmount ?? 0;
   
            var userStockAmount = databaseHelper.GetStocks(user, stockName);
            var newStockAmount = userStockAmount - stockAmount;

            databaseHelper.UpdateStocks(user, stockName, newStockAmount);
            databaseHelper.CommitTransaction(ref transaction);
        }
    }
}