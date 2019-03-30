using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        public async Task SetSellAmount()
        {
            // Sanity check
            CheckParams("amount", "stock");

            // Get params
            var user = await databaseHelper.GetUser(username, true);
            var sellAmountInDollars = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();

            logger.LogCommand(user, command, sellAmountInDollars, stockSymbol);

            // Doesn't make sense to have a sell amount of 0$
            if (sellAmountInDollars == 0)
            {
                throw new InvalidOperationException("Can't set a sell amount of 0");
            }

            // Check if existing trigger exists
            var existingSellTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (existingSellTrigger != null)
            {
                var newSellAmountInDollars = existingSellTrigger.BalanceChange + sellAmountInDollars;

                // If trigger has already been set, and this is an update on how much to buy
                await databaseHelper.SetTransactionBalanceChange(existingSellTrigger, newSellAmountInDollars);
            }
            else
            {
                // Create transaction with stockAmount = null, stockPrice = null
                await databaseHelper.CreateTransaction(user, stockSymbol, "SELL_TRIGGER", sellAmountInDollars, null, null, "pending");
            }
        }
    }
}