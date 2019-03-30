using System;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        /*
            Buy command flow:
            1- Get current user balance
            2- Get current stock price
            3- Calculate stock amount (only whole stocks)
            3- Insert buy into transactions table, *set pending flag to true*
         */
        public async Task Buy() 
        {
            CheckParams("amount", "stock");

            // Unpack JObject
            var amount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stock"].ToString();

            // Get users current balance
            var user = await databaseHelper.GetUser(username, true);

            // Log the command
            logger.LogCommand(user, command, amount, stockSymbol);
            
            if (user.PendingBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            // Get current stock price
            var stockPrice = await databaseHelper.GetStockPrice(user, stockSymbol);

            if (amount < stockPrice)
            {
                throw new InvalidOperationException("Not enough money for single stock purchase.");
            }

            var stockAmount = (int)(amount / stockPrice);
            var balanceChange = stockAmount * stockPrice * -1;

            await databaseHelper.CreateTransaction(user, stockSymbol, "BUY", balanceChange, stockAmount, stockPrice, "pending");
        } 
    }
}
