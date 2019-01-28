using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void Add(string userid, decimal amount) 
        {
            decimal balance = Helper.GetUserBalance(userid);
            balance += amount;
            if(Helper.UpdateUserBalance(userid, balance))
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