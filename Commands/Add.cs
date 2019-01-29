using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void Add(string userid, JObject commandParams) 
        {
            bool res;
            decimal amount = (decimal)commandParams["amount"];
            var balance = TransactionHelper.GetUserBalance(userid);
            if(balance == null)
            {
                //User does not exist
                Console.WriteLine("User does not exist, adding new user: {0}", userid);
                res = TransactionHelper.AddUser(userid, amount);
            }
            else
            {
                //Update existing user balance
                decimal newBalance = (decimal)balance + amount;
                res = TransactionHelper.UpdateUserBalance(userid, newBalance);
            }
            
            if(res)
            {
                Console.WriteLine("Updated user: {0}, balance: {1}", userid, balance);
            }
            else
            {
                Console.WriteLine("Error! Updating user: {0}, balance: {1} FAILED", userid, balance);
            }
        } 

    }
}