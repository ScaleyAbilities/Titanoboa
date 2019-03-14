using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void CancelSetSell(string username, JObject commandParams)
        {
            ParamHelper.ValidateParamsExist(commandParams, "stock");

            // Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            //  Get user
            var user = TransactionHelper.GetUser(username);

            Program.Logger.LogCommand(user, null, stockSymbol);

            // Get trigger to cancel
            var existingSellTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSellTrigger != null)
            {
                // Send new trigger to Twig
                dynamic twigTrigger = new JObject();

                // Populate JSON Object
                twigTrigger.Id = existingSellTrigger.Id;
                twigTrigger.User = existingSellTrigger.User;
                twigTrigger.Command = "CANCEL_TRIGGER";
                twigTrigger.StockSymbol = existingSellTrigger.StockSymbol;
                twigTrigger.StockAmount = existingSellTrigger.StockAmount;
                twigTrigger.StockPrice = existingSellTrigger.StockPrice;

                // TODO: Push twigTrigger to Rabbit Q


                // Cancel transaction & log
                // IF ACK from twig 
                if (true)
                {
                    // If trigger can be successfully cancelled, refund user's stocks
                    var refundedStocks = existingSellTrigger.StockAmount ?? 0;
                    if (refundedStocks != 0)
                    {
                        // Get users stocks
                        var numUserStocks = TransactionHelper.GetStocks(user, stockSymbol);

                        // Refund user
                        var newUserStocks = numUserStocks + refundedStocks;
                        TransactionHelper.UpdateStocks(user, stockSymbol, newUserStocks);
                    }
                    // Cancel transaction
                    TransactionHelper.DeleteTransaction(existingSellTrigger);
                }
                else
                {
                    throw new InvalidProgramException("The SELL trigger that is trying to be cancelled has already gone through!");
                }
            }
            else
            {
                throw new System.InvalidOperationException("No SELL_TRIGGER to cancel.");
            }
        }
    }
}