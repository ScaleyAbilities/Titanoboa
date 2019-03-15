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
            var amount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();

            logger.LogCommand(user, amount, stockSymbol);
            
            // Check if existing trigger exists
            Transaction sellTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "SELL_TRIGGER");
            if (sellTrigger != null)
            {
                var newAmount = sellTrigger.BalanceChange + amount;
                await databaseHelper.SetTransactionBalanceChange(sellTrigger, newAmount);
            }
            else
            {
                sellTrigger = await databaseHelper.CreateTransaction(user, stockSymbol, "SELL_TRIGGER", amount, null, null, "trigger");
            }
        }
    }
}