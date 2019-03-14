using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void CancelSetBuy(string username, JObject commandParams)
        {
            ParamHelper.ValidateParamsExist(commandParams, "stock");

            // Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            // Get user
            var user = TransactionHelper.GetUser(username);

            Program.Logger.LogCommand(user, null, stockSymbol);

            // Get trigger to cancel
            var existingBuyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingBuyTrigger != null)
            {
                // Send new trigger to Twig
                dynamic twigTrigger = new JObject();

                // Populate JSON Object
                twigTrigger.User = existingBuyTrigger.User;
                twigTrigger.Command = "CANCEL";
                twigTrigger.StockSymbol = existingBuyTrigger.StockSymbol;
                twigTrigger.StockPrice = existingBuyTrigger.StockPrice;

                // TODO: Push twigTrigger to Rabbit Q
                
                // IF ACK from twig 
                if (true)
                {
                    // If trigger can be successfully cancelled, refund user's money
                    var refund = existingBuyTrigger.BalanceChange;
                    if (refund != 0)
                    {
                        // Refund user
                        var newBalance = user.Balance + refund;
                        TransactionHelper.UpdateUserBalance(ref user, newBalance);
                    }
                    // Cancel transaction
                    TransactionHelper.DeleteTransaction(existingBuyTrigger);
                }
                else
                {
                    throw new InvalidProgramException("The BUY_TRIGGER that is trying to be cancelled has already gone through!");
                }
            }
            else
            {
                throw new System.InvalidOperationException("No BUY_TRIGGER to cancel.");
            }

        }

    }
}