using MySql.Data.MySqlClient;
using System;

namespace Titanoboa
{
    public static partial class Commands 
    {
        /*
            CommitBuy command flow:
            1- Get most recent buy (within 60 seconds), 
            2- Remove funds
            3- Add stock amounts
            3- Update buy in transactions table, *set pending flag to false, and update timestamp*
         */
        public static void CommitBuy(string username) {

            var user = TransactionHelper.GetUser(username, false);
            var transaction = TransactionHelper.GetLatestPendingTransaction(user, "BUY");

            if(transaction == null) {
                throw new InvalidOperationException("No pending BUY transactions to commit.");
            }

            var newBalance = user.Balance - Math.Abs(transaction.BalanceChange);
            TransactionHelper.UpdateUserBalance(ref user, newBalance);

            var stockName = transaction.StockSymbol;
            var stockAmount = transaction.StockAmount;

            var userStockAmount = TransactionHelper.GetStocks(user, stockName);
            var newStockAmount = stockAmount + userStockAmount;

            TransactionHelper.UpdateStocks(user, stockName, userStockAmount);
            TransactionHelper.CommitTransaction(transaction);
        }
    }
}