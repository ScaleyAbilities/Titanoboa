using System.Threading.Tasks;

namespace Titanoboa
{
    public partial class CommandHandler 
    {
        /*
            CancelSell command flow:
            1- Get most recent sell (within 60 seconds), 
            2- Delete transaction from transaction table
         */
         public async Task CancelSell() 
         {
            var user = await databaseHelper.GetUser(username, false);

            logger.LogCommand(user, command);

            var transaction = await databaseHelper.GetLatestPendingTransaction(user, "SELL");
            if (transaction != null)
            {
                await databaseHelper.DeleteTransaction(transaction);
            }
         }
        
    }
}