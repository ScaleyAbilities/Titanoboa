using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void Add(string username, JObject commandParams) 
        {
            ParamHelper.ValidateParamsExist(commandParams, "amount");
            
            decimal amount = (decimal)commandParams["amount"];
            var user = TransactionHelper.GetUser(username);

            // Log this command
            Program.Logger.LogCommand(user, amount);

            // Update existing user balance
            decimal newBalance = user.Balance + amount;
            var transaction = TransactionHelper.CreateTransaction(user, null, "ADD", amount);

            TransactionHelper.UpdateUserBalance(ref user, newBalance);
        } 

    }
}