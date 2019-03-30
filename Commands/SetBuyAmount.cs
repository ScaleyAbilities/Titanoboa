using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        public async Task SetBuyAmount()
        {
            // Sanity check
            CheckParams("amount", "stock");

            // Get params
            var user = await databaseHelper.GetUser(username, true);
            var buyAmountInDollars = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();

            logger.LogCommand(user, command, buyAmountInDollars, stockSymbol);

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
            var existingBuyTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (existingBuyTrigger != null)
            {
                var newBuyAmountInDollars = existingBuyTrigger.BalanceChange + buyAmountInDollars;
                await databaseHelper.SetTransactionBalanceChange(existingBuyTrigger, newBuyAmountInDollars);
            }
            else
            {
                // Create transaction with stockAmount = null, stockPrice = null
                await databaseHelper.CreateTransaction(user, stockSymbol, "BUY_TRIGGER", buyAmountInDollars, null, null, "pending");
            }
        }
    }
}