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

            // Unpack JObject
            var user = TransactionHelper.GetUser(username, true);
            var sellAmountInDollars = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();
            
            // Log Command
            Program.Logger.LogCommand(user, sellAmountInDollars, stockSymbol);

            // Doesn't make sense to have a sell amount of 0$
            if (sellAmountInDollars == 0)
            {
                throw new InvalidOperationException("Can't set a sell amount of 0");
            }

            // Check if existing trigger exists
            var existingSellTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSellTrigger != null)
            {
                var newSellAmountInDollars = existingSellTrigger.BalanceChange + sellAmountInDollars;

                // If trigger has already been set, and this is an update on how much to buy
                TransactionHelper.SetTransactionBalanceChange(ref existingSellTrigger, newSellAmountInDollars);
            }
            else
            {
                // Create transaction with stockAmount = null, stockPrice = null
                TransactionHelper.CreateTransaction(user, stockSymbol, "SELL_TRIGGER", sellAmountInDollars, null, null, "trigger");
            }
        }
    }
}