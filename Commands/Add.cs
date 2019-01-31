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

            //Update existing user balance
            decimal newBalance = user.Balance + amount;
            TransactionHelper.UpdateUserBalance(ref user, newBalance);

            Console.WriteLine("Updated user: {0}, balance: {1}", user.Username, user.Balance);
        } 

    }
}