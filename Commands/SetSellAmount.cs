using System;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void SetSellAmount(string username, JObject commandParams)
        {
            // Sanity check
            ParamHelper.ValidateParamsExist(commandParams, "amount", "stock");

            // Get params
            var user = TransactionHelper.GetUser(username, true);
            var amount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();
            
            // Check if existing trigger exists
            Transaction existingSellTrigger = TransactionHelper.GetTrigger(user, stockSymbol, "SELL_TRIGGER");
            if (existingSellTrigger != null)
            {
                var newAmount = existingSellTrigger.BalanceChange + amount;
                TransactionHelper.SetTransactionBalanceChange(ref existingSellTrigger, newAmount);
            }
            else
            {
                TransactionHelper.CreateTransaction(user, stockSymbol, "SELL_TRIGGER", amount, null, null, "trigger");
            }
            
        }
    }
}