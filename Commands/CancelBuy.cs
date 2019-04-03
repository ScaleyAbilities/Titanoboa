using System;
using System.Threading.Tasks;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        /*
            CancelBuy command flow:
            1- Get most recent buy (within 60 seconds), 
            2- Delete transaction from transaction table
         */
        public async Task CancelBuy() 
        {
            var user = await databaseHelper.GetUser(username, false);

            logger.LogCommand(user, command);

            var transaction = await databaseHelper.GetLatestPendingTransaction(user, "BUY");
            if (transaction != null)
            {
                await databaseHelper.DeleteTransaction(transaction);
            }
            else 
            {
                throw new System.InvalidOperationException("No pending BUY to cancel.");
            }
        }
    }
}