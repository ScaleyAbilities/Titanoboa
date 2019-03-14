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
            Transaction ExistingBuyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (ExistingBuyTrigger != null)
            {
                var newAmount = ExistingBuyTrigger.BalanceChange + amount;
                TransactionHelper.SetTransactionBalanceChange(ref ExistingBuyTrigger, newAmount);

                // If trigger has already been set, and this is an update on how much to buy
                if(ExistingBuyTrigger.StockPrice != null)
                {
                    // Send new trigger to Twig
                    dynamic twigTrigger = new JObject();

                    // Populate JSON Object
                    twigTrigger.Id = ExistingBuyTrigger.Id;
                    twigTrigger.User = ExistingBuyTrigger.User;
                    twigTrigger.Command = "UPDATE_BUY_TRIGGER";
                    twigTrigger.StockSymbol = ExistingBuyTrigger.StockSymbol;
                    twigTrigger.StockAmount = ExistingBuyTrigger.StockAmount;
                    twigTrigger.StockPrice = ExistingBuyTrigger.StockPrice;
                }   
            }
            else
            {
                ExistingBuyTrigger = TransactionHelper.CreateTransaction(user, stockSymbol, "BUY_TRIGGER", amount, null, null, "trigger");
            }
        }
    }
}