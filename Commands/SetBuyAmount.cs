using System;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void SetBuyAmount(string username, JObject commandParams)
        {
            // Sanity check
            ParamHelper.ValidateParamsExist(commandParams, "amount", "stock");

            // Get params
            var user = TransactionHelper.GetUser(username, true); // Get user with pending balance
            var buyAmountInDollars = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();

            // Log Command
            Program.Logger.LogCommand(user, buyAmountInDollars, stockSymbol);

            // Doesn't make sense to buy 0$ worth of stocks
            if (buyAmountInDollars == 0)
            {
                throw new System.InvalidOperationException("Cannot buy 0 dollars worth of stocks.");
            }

            // Not enough funds for trigger
            if (user.PendingBalance < buyAmountInDollars)
            {
                throw new InvalidOperationException("Insufficient funds for SET_BUY_AMOUNT.");
            }

            // Check if existing trigger exists and update amount, else create new trigger
            var existingBuyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingBuyTrigger != null)
            {
                var newBuyAmountInDollars = existingBuyTrigger.BalanceChange + buyAmountInDollars;
                TransactionHelper.SetTransactionBalanceChange(ref existingBuyTrigger, newBuyAmountInDollars);
            }
            else
            {
                // Create transaction with stockAmount = null, stockPrice = null
                TransactionHelper.CreateTransaction(user, stockSymbol, "BUY_TRIGGER", buyAmountInDollars, null, null, "pending");
            }
        }
    }
}