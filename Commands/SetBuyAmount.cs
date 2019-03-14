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
            TransactionHelper.UpdateUserBalance(ref user, newBalance);

            // Check if existing trigger exists and update amount, else create new trigger
            Transaction existingBuyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingBuyTrigger != null)
            {
                var newAmount = existingBuyTrigger.BalanceChange + amount;

                // If trigger has already been set, and this is an update on how much to buy
                if(existingBuyTrigger.StockPrice != null)
                {
                    // Send new trigger to Twig
                    dynamic twigTrigger = new JObject();

                    // Populate JSON Object
                    twigTrigger.Id = existingBuyTrigger.Id;
                    twigTrigger.User = existingBuyTrigger.User;
                    twigTrigger.Command = "UPDATE_BUY_TRIGGER";
                    twigTrigger.StockSymbol = existingBuyTrigger.StockSymbol;
                    twigTrigger.StockAmount = newAmount;
                    twigTrigger.StockPrice = existingBuyTrigger.StockPrice;

                    // TODO: Send twigTrigger to Rabbit Q 
                    // IF ACK
                    if(true)
                    {   
                        TransactionHelper.SetTransactionBalanceChange(ref existingBuyTrigger, newAmount);
                    }
                    else 
                    {
                        throw new InvalidProgramException("Tried to update trigger amount, but trigger is in the process of being completed!");
                    } 
                }   
            }
            else
            {
                TransactionHelper.CreateTransaction(user, stockSymbol, "BUY_TRIGGER", amount, null, null, "trigger");
            }
        }
    }
}