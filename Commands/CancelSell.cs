namespace Titanoboa
{
    public partial class CommandHandler 
    {
        /*
            CancelSell command flow:
            1- Get most recent sell (within 60 seconds), 
            2- Delete transaction from transaction table
         */
         public void CancelSell() 
         {
            var user = databaseHelper.GetUser(username, false);

            logger.LogCommand(user);

            var transaction = databaseHelper.GetLatestPendingTransaction(user, "SELL");
            if (transaction != null)
            {
                databaseHelper.DeleteTransaction(transaction);
            }
         }
        
    }
}