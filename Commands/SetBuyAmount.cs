using System;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void SetBuyAmount(string username, JObject commandParams) {

            ParamHelper.ValidateParamsExist(commandParams, "amount", "stock");

            var user = TransactionHelper.GetUser(username, true);
            var amount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();
            
            // Not enough funds for trigger
            if(user.PendingBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds for trigger.");  
            } 

            var newBalance = user.PendingBalance - amount;

            TransactionHelper.AddTransaction(user, stockSymbol, "BUY_TRIGGER", amount, null, "trigger");


        }
        
    }
}