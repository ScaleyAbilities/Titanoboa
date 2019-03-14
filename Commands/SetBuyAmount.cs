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
            var user = TransactionHelper.GetUser(username, true);
            var buyAmount = (decimal)commandParams["amount"];
            if (buyAmount == 0)
            {
                throw new System.InvalidOperationException("Cannot buy 0 dollars worth of stocks.");
            }

            var stockSymbol = commandParams["stock"].ToString();

            Program.Logger.LogCommand(user, buyAmount, stockSymbol);

            // Not enough funds for trigger
            if (user.PendingBalance < buyAmount)
            {
                throw new InvalidOperationException("Insufficient funds for SET_BUY_AMOUNT.");
            }

            // Reserve funds for trigger
            var newBalance = user.Balance - buyAmount;
            TransactionHelper.UpdateUserBalance(ref user, newBalance);

            // Check if existing trigger exists and update amount, else create new trigger
            Transaction existingBuyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingBuyTrigger != null)
            {
                var newAmount = existingBuyTrigger.BalanceChange + buyAmount;

                // If trigger has already been set, and this is an update on how much to buy
                if (existingBuyTrigger.StockPrice != null)
                {
                    TransactionHelper.SetTransactionBalanceChange(ref existingBuyTrigger, newAmount);
                }
            }
            else
            {
                TransactionHelper.CreateTransaction(user, stockSymbol, "BUY_TRIGGER", buyAmount, null, null, "trigger");
            }
        }
    }
}