using System;

namespace Titanoboa
{
    public static partial class Commands
    {
        /*
            CancelBuy command flow:
            1- Get most recent buy (within 60 seconds), 
            2- Delete transaction from transaction table
         */
        public static void CancelBuy(string username) 
        {
            var user = TransactionHelper.GetUser(username, false);
            var transaction = TransactionHelper.GetLatestPendingTransaction(user, "BUY");
            TransactionHelper.DeleteTransaction(transaction);
        }
    }
}