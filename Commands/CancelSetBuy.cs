using System;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        public async Task CancelSetBuy()
        {
            CheckParams("stock");

            // Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            // Get user
            var user = await databaseHelper.GetUser(username);

            logger.LogCommand(user, command, null, stockSymbol);

            // Get trigger to cancel
            var existingSetBuyTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingSetBuyTrigger == null)
            {
                throw new InvalidOperationException("Can't cancel BUY_TRIGGER: Trigger doesn't exist");
            }
            else if (existingSetBuyTrigger.Type == "completed")
            {
                throw new InvalidOperationException("Can't cancel BUY_TRIGGER: Trigger has already gone through!");
            }

            // Cancel transaction
            await databaseHelper.DeleteTransaction(existingSetBuyTrigger);

            // Send new trigger to Twig
            JObject twigTrigger = new JObject();
            JObject twigParams = new JObject();
            twigTrigger.Add("usr", username);
            twigTrigger.Add("cmd", "CANCEL_BUY");
            twigParams.Add("stock", existingSetBuyTrigger.StockSymbol);
            twigTrigger.Add("params", twigParams);
            RabbitHelper.PushTrigger(twigTrigger);
        }
    }
}