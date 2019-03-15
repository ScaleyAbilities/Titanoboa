using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        public async Task CancelSetBuy() {
            CheckParams("stock");

            // Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            // Get user
            var user = await databaseHelper.GetUser(username);

            logger.LogCommand(user, null, stockSymbol);

            // Get trigger to cancel
            var existingSetBuyTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingSetBuyTrigger != null)
            {
                var refund = existingSetBuyTrigger.BalanceChange;

                // Refund user
                var newBalance = user.Balance + refund;
                await databaseHelper.UpdateUserBalance(user, newBalance);

                // Cancel transaction
                await databaseHelper.DeleteTransaction(existingSetBuyTrigger);
            }
            
        }
        
    }
}