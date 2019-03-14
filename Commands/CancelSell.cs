namespace Titanoboa
{
    public partial class Commands 
    {
        /*
            CancelSell command flow:
            1- Get most recent sell (within 60 seconds), 
            2- Delete transaction from transaction table
         */
         public void CancelSell(string username) 
         {
            var user = databaseHelper.GetUser(username, false);

            Program.Logger.LogCommand(user);

            var transaction = databaseHelper.GetLatestPendingTransaction(user, "SELL");
            if (transaction != null)
            {
                databaseHelper.DeleteTransaction(transaction);
            }
         }
        
    }
}