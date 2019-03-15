using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        public async Task CancelSetSell() {
            CheckParams("stock");

            // Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            //  Get user
            var user = await databaseHelper.GetUser(username);

            logger.LogCommand(user, null, stockSymbol);

            // Get trigger to cancel
            var existingSetSellTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSetSellTrigger != null)
            {
                var refundedStocks = existingSetSellTrigger.StockAmount ?? 0;

                // Get users stocks
                var numUserStocks = await databaseHelper.GetStocks(user, stockSymbol);

                // Refund user
                var newUserStocks = numUserStocks + refundedStocks;
                await databaseHelper.UpdateStocks(user, stockSymbol, newUserStocks);

                // Cancel transaction & log
                await databaseHelper.DeleteTransaction(existingSetSellTrigger);
            }
        }
        
    }
}