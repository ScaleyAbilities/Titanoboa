using System;
using System.Data;
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

            logger.LogCommand(user, command, null, stockSymbol);

            // Get trigger to cancel
            var existingSetSellTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSetSellTrigger == null)
            {
                throw new InvalidOperationException("Can't cancel SELL_TRIGGER: Trigger doesn't exist");
            }
            else if (existingSetSellTrigger.Type == "completed")
            {
                throw new InvalidOperationException("Can't cancel SELL_TRIGGER: Trigger has already gone through!");
            }

            // Cancel transaction
            await databaseHelper.DeleteTransaction(existingSetSellTrigger);

            // Send new trigger to Twig
            JObject twigTrigger = new JObject();
            JObject twigParams = new JObject();
            twigTrigger.Add("usr", username);
            twigTrigger.Add("cmd", "CANCEL_SELL");
            twigParams.Add("stock", existingSetSellTrigger.StockSymbol);
            twigTrigger.Add("params", twigParams);
            RabbitHelper.PushTrigger(twigTrigger);
        }
    }
}