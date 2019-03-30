using System;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        public async Task Add()
        {
            CheckParams("amount");
            
            decimal amount = (decimal)commandParams["amount"];
            var user = await databaseHelper.GetUser(username);

            // Log this command
            logger.LogCommand(user, command, amount);

            // Update existing user balance
            decimal newBalance = user.Balance + amount;
            await databaseHelper.CreateTransaction(user, null, "ADD", amount);
            await databaseHelper.UpdateUserBalance(user, newBalance);
        } 

    }
}