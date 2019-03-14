using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void CancelSetSell(string username, JObject commandParams)
        {
            ParamHelper.ValidateParamsExist(commandParams, "stock");

            // Get params
            var user = TransactionHelper.GetUser(username);
            var stockSymbol = commandParams["stock"].ToString();

            // Log Command
            Program.Logger.LogCommand(user, null, stockSymbol);

            // Get trigger to cancel
            var existingSellTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSellTrigger == null)
            {
                throw new InvalidOperationException("Can't cancel SELL_TRIGGER: Trigger doesn't exist");
            }
            else if (existingSellTrigger.Type == "completed")
            {
                throw new InvalidOperationException("Can't commit SELL_TRIGGER: Trigger has already gone through!");
            }

            // Cancel transaction
            TransactionHelper.DeleteTransaction(existingSellTrigger);

            // Send new trigger to Twig
            JObject twigTrigger = new JObject();
            twigTrigger["User"] = existingSellTrigger.User.Username;
            twigTrigger["Command"] = "CANCEL_SELL";
            twigTrigger["StockSymbol"] = existingSellTrigger.StockSymbol;
            RabbitHelper.PushCommand(twigTrigger);
        }
    }
}