using System;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class Commands
    {
        public void SetBuyAmount(string username, JObject commandParams)
        {
            // Sanity check
            ParamHelper.ValidateParamsExist(commandParams, "amount", "stock");

            // Get params
            var user = databaseHelper.GetUser(username, true);
            var amount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();

            Program.Logger.LogCommand(user, amount, stockSymbol);
            
            // Not enough funds for trigger
            if (user.PendingBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds for SET_BUY_AMOUNT.");  
            } 

            // Reserve funds for trigger
            var newBalance = user.Balance - amount;
            databaseHelper.UpdateUserBalance(ref user, newBalance);

            // Check if existing trigger exists and update amount, else create new trigger
            Transaction buyTrigger = databaseHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (buyTrigger != null)
            {
                var newAmount = buyTrigger.BalanceChange + amount;
                databaseHelper.SetTransactionBalanceChange(ref buyTrigger, newAmount);
            }
            else
            {
                buyTrigger = databaseHelper.CreateTransaction(user, stockSymbol, "BUY_TRIGGER", amount, null, null, "trigger");
            }
        }
    }
}