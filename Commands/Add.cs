using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        public void Add()
        {
            CheckParams("amount");
            
            decimal amount = (decimal)commandParams["amount"];
            var user = databaseHelper.GetUser(username);

            // Log this command
            logger.LogCommand(user, amount);

            // Update existing user balance
            decimal newBalance = user.Balance + amount;
            var transaction = databaseHelper.CreateTransaction(user, null, "ADD", amount);

            databaseHelper.UpdateUserBalance(ref user, newBalance);
        } 

    }
}