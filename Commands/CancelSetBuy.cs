using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void CancelSetBuy(string username, JObject commandParams)
        {
            // Sanity check
            ParamHelper.ValidateParamsExist(commandParams, "stock");

            // Get params
            var user = TransactionHelper.GetUser(username);
            var stockSymbol = commandParams["stock"].ToString();

            // Log Command
            Program.Logger.LogCommand(user, null, stockSymbol);

            // Get trigger to cancel
            var existingBuyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingBuyTrigger == null)
            {
                throw new InvalidOperationException("Can't cancel BUY_TRIGGER: Trigger doesn't exist");
            }
            else if (existingBuyTrigger.Type == "completed")
            {
                throw new InvalidOperationException("Can't cancel BUY_TRIGGER: Trigger has already gone through!");
            }

            // Cancel transaction
            TransactionHelper.DeleteTransaction(existingBuyTrigger);

            // Send new trigger to Twig
            JObject twigTrigger = new JObject();
            JObject twigParams = new JObject();
            twigTrigger.Add("usr", username);
            twigTrigger.Add("cmd", "CANCEL_BUY");
            twigParams.Add("stock", existingBuyTrigger.StockSymbol);
            twigTrigger.Add("params", twigParams);
            RabbitHelper.PushTrigger(twigTrigger);
        }
    }
}