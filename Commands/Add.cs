using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class Commands
    {
        public void Add(string username, JObject commandParams) 
        {
            ParamHelper.ValidateParamsExist(commandParams, "amount");
            
            decimal amount = (decimal)commandParams["amount"];
            var user = databaseHelper.GetUser(username);

            // Log this command
            Program.Logger.LogCommand(user, amount);

            // Update existing user balance
            decimal newBalance = user.Balance + amount;
            var transaction = databaseHelper.CreateTransaction(user, null, "ADD", amount);

            databaseHelper.UpdateUserBalance(ref user, newBalance);
        } 

    }
}