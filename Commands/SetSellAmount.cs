using System;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class Commands
    {
        public void SetSellAmount(string username, JObject commandParams)
        {
            // Sanity check
            ParamHelper.ValidateParamsExist(commandParams, "amount", "stock");

            // Get params
            var user = databaseHelper.GetUser(username, true);
            var amount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();

            Program.Logger.LogCommand(user, amount, stockSymbol);
            
            // Check if existing trigger exists
            Transaction sellTrigger = databaseHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (sellTrigger != null)
            {
                var newAmount = sellTrigger.BalanceChange + amount;
                databaseHelper.SetTransactionBalanceChange(ref sellTrigger, newAmount);
            }
            else
            {
                sellTrigger = databaseHelper.CreateTransaction(user, stockSymbol, "SELL_TRIGGER", amount, null, null, "trigger");
            }
        }
    }
}