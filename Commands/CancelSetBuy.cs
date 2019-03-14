using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class Commands
    {
        public void CancelSetBuy(string username, JObject commandParams) {
            ParamHelper.ValidateParamsExist(commandParams, "stock");

            // Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            // Get user
            var user = databaseHelper.GetUser(username);

            Program.Logger.LogCommand(user, null, stockSymbol);

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