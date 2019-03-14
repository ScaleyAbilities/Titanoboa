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

            Program.Logger.LogCommand(user, amount, stockSymbol);

            // Check if existing trigger exists
            Transaction existingSellTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSellTrigger != null)
            {
                var newAmount = existingSellTrigger.BalanceChange + amount;

                // If trigger has already been set, and this is an update on how much to buy
                if (existingSellTrigger.StockPrice != null)
                {
                    TransactionHelper.SetTransactionBalanceChange(ref existingSellTrigger, newAmount);
                }
            }
            else
            {
                TransactionHelper.CreateTransaction(user, stockSymbol, "SELL_TRIGGER", amount, null, null, "trigger");
            }
        }
    }
}