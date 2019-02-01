using System;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void SetBuyAmount(string username, JObject commandParams) {

            // Sanity check
            ParamHelper.ValidateParamsExist(commandParams, "amount", "stock");


            // Get params
            var user = TransactionHelper.GetUser(username, true);
            var amount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();
            
            // Not enough funds for trigger
            if(user.PendingBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds for SET_BUY_AMOUNT.");  
            } 

            // Reserve funds for trigger
            var newBalance = user.Balance - amount;
            TransactionHelper.UpdateUserBalance(ref user, newBalance);

            // Check if existing trigger exists
            Transaction existingBuyTrigger = TransactionHelper.GetTrigger(user, stockSymbol);
            if(existingBuyTrigger != null) {
                existingBuyTrigger.BalanceChange += amount;
                TransactionHelper.UpdateTransaction(ref existingBuyTrigger);
            }
            else
            {
                TransactionHelper.AddTransaction(user, stockSymbol, "BUY_TRIGGER", amount, null, "trigger");
            }

            
        }
        
    }
}