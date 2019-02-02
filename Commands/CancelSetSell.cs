using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void CancelSetSell(string username, JObject commandParams) {
            ParamHelper.ValidateParamsExist(commandParams, "stock");

            // Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            //  Get user
            var user = TransactionHelper.GetUser(username);

            // Get trigger to cancel
            var existingSetSellTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSetSellTrigger != null)
            {
                var refundedStocks = existingSetSellTrigger.StockAmount ?? 0;

                // Get users stocks
                var numUserStocks = TransactionHelper.GetStocks(user, stockSymbol);

                // Refund user
                var newUserStocks = numUserStocks + refundedStocks;
                TransactionHelper.UpdateStocks(user, stockSymbol, newUserStocks);

                // Cancel transaction & log
                TransactionHelper.DeleteTransaction(existingSetSellTrigger);
            }
        }
        
    }
}