using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void CommitBuyTrigger(string username, JObject commandParams)
        {
            // Sanity check
            ParamHelper.ValidateParamsExist(commandParams, "price", "stock");

            // Get params
            var user = TransactionHelper.GetUser(username);
            var committedBuyPrice = (decimal)commandParams["price"];
            var stockSymbol = commandParams["stock"].ToString();

            // Log command
            Program.Logger.LogCommand(user, committedBuyPrice, stockSymbol);

            // Can't have buy price of 0
            if (committedBuyPrice <= 0)
            {
                throw new InvalidOperationException("Can't have a sell price of 0.");
            }

            // Get the existing trigger transaction to be committed
            var existingBuyTrigger = TransactionHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingBuyTrigger == null)
            {
                throw new InvalidOperationException("Can't commit BUY_TRIGGER: Trigger may have been cancelled!");
            }

            var maxToSpend = existingBuyTrigger.BalanceChange;
            if(maxToSpend <= committedBuyPrice)
            {
                throw new InvalidOperationException("The price was too high! Trigger was trying to buy 0 stocks");
            }

            // Check to make sure trigger worked properly
            var maxBuyPrice = existingBuyTrigger.StockPrice;
            if(maxBuyPrice < committedBuyPrice) 
            {
                throw new InvalidProgramException("Program Error! Trigger bought for more than the user's max price");
            }

            // Calculate how many stocks to buy take actual amount out of user balance
            var numStocksToBuy = (int)Math.Floor(maxToSpend / committedBuyPrice);
            var totalCost = numStocksToBuy * committedBuyPrice;
            var newUserBalance = user.Balance - totalCost;
            TransactionHelper.UpdateUserBalance(ref user, newUserBalance);

            // Update and commit the transaction
            TransactionHelper.SetTransactionBalanceChange(ref existingBuyTrigger, totalCost); 
            TransactionHelper.SetTransactionStockPrice(ref existingBuyTrigger, committedBuyPrice);
            TransactionHelper.SetTransactionNumStocks(ref existingBuyTrigger, numStocksToBuy);
            TransactionHelper.CommitTransaction(ref existingBuyTrigger);
        }
    }
}