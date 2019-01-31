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
        public static void CommitBuy(string userid) {
            
            var transaction = TransactionHelper.GetLatestPendingTransaction(userid, "BUY");

            if(transaction == null) {
                throw new InvalidOperationException("No pending BUY transactions to commit.");
            }

            var user = TransactionHelper.GetUser(userid, false);

            var newBalance = user.Balance - transaction.BalanceChance;
            TransactionHelper.UpdateUserBalance(ref user, newBalance);

            var stockName = transaction.StockSymbol;
            var stockAmount = transaction.StockAmount;

            TransactionHelper.UpdateStock(ref user, stockName, stockAmount);
            TransactionHelper.CommitTransaction(ref transaction);

        }
    }
}