using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void CancelSetBuy(string username, JObject commandParams) {
            ParamHelper.ValidateParamsExist(commandParams, "stock");

            // Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            // Get user
            var user = TransactionHelper.GetUser(username);

            Program.Logger.LogCommand("CANCEL_SET_BUY", user, null, stockSymbol);

            // Get trigger to cancel
            var existingSetBuyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingSetBuyTrigger != null)
            {
                var refund = existingSetBuyTrigger.BalanceChange;

                // Refund user
                var newBalance = user.Balance + refund;
                TransactionHelper.UpdateUserBalance(ref user, newBalance);

                // Cancel transaction
                TransactionHelper.DeleteTransaction(existingSetBuyTrigger);

                Program.Logger.LogEvent(Logger.EventType.System, "CANCEL_SET_BUY", "Cancelled SET_BUY trigger", user, existingSetBuyTrigger.BalanceChange, existingSetBuyTrigger.StockSymbol);
            }
            
        }
        
    }
}