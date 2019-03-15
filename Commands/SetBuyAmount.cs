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
            var amount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();

            logger.LogCommand(user, amount, stockSymbol);
            
            // Not enough funds for trigger
            if (user.PendingBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds for SET_BUY_AMOUNT.");  
            } 

            // Reserve funds for trigger
            var newBalance = user.Balance - amount;
            await databaseHelper.UpdateUserBalance(user, newBalance);

            // Check if existing trigger exists and update amount, else create new trigger
            Transaction buyTrigger = await databaseHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (buyTrigger != null)
            {
                var newAmount = buyTrigger.BalanceChange + amount;
                await databaseHelper.SetTransactionBalanceChange(buyTrigger, newAmount);
            }
            else
            {
                buyTrigger = await databaseHelper.CreateTransaction(user, stockSymbol, "BUY_TRIGGER", amount, null, null, "trigger");
            }
        }
    }
}