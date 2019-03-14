using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        public void CancelSetBuy() {
            CheckParams("stock");

            // Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            // Get user
            var user = databaseHelper.GetUser(username);

            logger.LogCommand(user, null, stockSymbol);

            // Get trigger to cancel
            var existingSetBuyTrigger = databaseHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingSetBuyTrigger != null)
            {
                var refund = existingSetBuyTrigger.BalanceChange;

                // Refund user
                var newBalance = user.Balance + refund;
                databaseHelper.UpdateUserBalance(ref user, newBalance);

                // Cancel transaction
                databaseHelper.DeleteTransaction(existingSetBuyTrigger);
            }
            
        }
        
    }
}