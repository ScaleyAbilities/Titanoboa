using System;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        /*
            CommitSell command flow:
            1- Get most recent Sell (within 60 seconds), 
            2- Add funds
            3- Remove stock amounts
            3- Update buy in transactions table, *set pending flag to false, and update timestamp*
         */
        public static void CommitSell(string username)
        {
            var user = TransactionHelper.GetUser(username, false);
            
            Program.Logger.LogCommand(user);

            var transaction = TransactionHelper.GetLatestPendingTransaction(user, "SELL");
            if (transaction == null)
            {
                throw new InvalidOperationException("No pending SELL transactions to commit.");
            }

            var newBalance = user.Balance + Math.Abs(transaction.BalanceChange);
            TransactionHelper.UpdateUserBalance(ref user, newBalance);

            var stockName = transaction.StockSymbol;
            var stockAmount = transaction.StockAmount ?? 0;
   
            var userStockAmount = TransactionHelper.GetStocks(user, stockName);
            var newStockAmount = userStockAmount - stockAmount;

            TransactionHelper.UpdateStocks(user, stockName, newStockAmount);
            TransactionHelper.CommitTransaction(ref transaction);
        }
    }
}