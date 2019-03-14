using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class Commands
    {
        public void CancelSetSell(string username, JObject commandParams) {
            ParamHelper.ValidateParamsExist(commandParams, "stock");

            // Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            //  Get user
            var user = databaseHelper.GetUser(username);

            Program.Logger.LogCommand(user, null, stockSymbol);

            // Get trigger to cancel
            var existingSetSellTrigger = databaseHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSetSellTrigger != null)
            {
                var refundedStocks = existingSetSellTrigger.StockAmount ?? 0;

                // Get users stocks
                var numUserStocks = databaseHelper.GetStocks(user, stockSymbol);

                // Refund user
                var newUserStocks = numUserStocks + refundedStocks;
                databaseHelper.UpdateStocks(user, stockSymbol, newUserStocks);

                // Cancel transaction & log
                databaseHelper.DeleteTransaction(existingSetSellTrigger);
            }
        }
        
    }
}