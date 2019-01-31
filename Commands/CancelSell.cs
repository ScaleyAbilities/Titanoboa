namespace Titanoboa
{
    public static partial class Commands 
    {
        /*
            CancelSell command flow:
            1- Get most recent sell (within 60 seconds), 
            2- Delete transaction from transaction table
         */
         public static void CancelSell(string username) 
         {
            var user = TransactionHelper.GetUser(username, false);
            var transaction = TransactionHelper.GetLatestPendingTransaction(user, "SELL");
            TransactionHelper.DeleteTransaction(transaction);
         }
        
    }
}