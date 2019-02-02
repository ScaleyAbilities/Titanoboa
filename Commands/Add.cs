using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
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
            Program.Logger.LogCommand("ADD", user, amount);

            // Update existing user balance
            decimal newBalance = user.Balance + amount;
            var transaction = TransactionHelper.CreateTransaction(user, null, "ADD", amount);

            // Log transaction
            Program.Logger.LogTransaction(user, transaction);

            TransactionHelper.UpdateUserBalance(ref user, newBalance);
        } 

    }
}