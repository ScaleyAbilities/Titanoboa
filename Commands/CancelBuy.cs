using System;

namespace Titanoboa
{
    public partial class Commands
    {
        /*
            CancelBuy command flow:
            1- Get most recent buy (within 60 seconds), 
            2- Delete transaction from transaction table
         */
        public void CancelBuy(string username) 
        {
            var user = databaseHelper.GetUser(username, false);

            Program.Logger.LogCommand(user);

            var transaction = databaseHelper.GetLatestPendingTransaction(user, "BUY");
            if (transaction != null)
            {
                databaseHelper.DeleteTransaction(transaction);
            }
            else 
            {
                throw new System.InvalidOperationException("No pending BUY to cancel.");
            }
        }
    }
}