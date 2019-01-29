using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class TransactionHelper
    {
        public static bool AddTransaction(string userid, decimal balance, string stockSymbol, string commandText, decimal balanceChange, int stockAmount, bool pending){
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.Prepare();
            command.CommandText = @"INSERT INTO transactions (userid, balance, stocksymbol, command, balancechange, stockamount, pending) 
                                    values ('@userid', '@balance', '@stocksymbol', '@command', '@balancechange', '@stockamount', '@pending')";
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@stocksymbol", stockSymbol);
            command.Parameters.AddWithValue("@command", commandText);
            command.Parameters.AddWithValue("@balancechange", balanceChange);
            command.Parameters.AddWithValue("@stockamount", stockAmount);
            command.Parameters.AddWithValue("@pending", pending);
            return SqlHelper.ExcecuteNonQuerySqlCommand(command);
        }

        internal static decimal GetStockPrice(string stockSymbol)
        {
            throw new NotImplementedException();
        }

        public static bool AddUser(string userid, decimal balance) {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "INSERT INTO users (balance, userid) Values ('@balance', '@userid')";
            command.Prepare();
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@userid", userid);
            return SqlHelper.ExcecuteNonQuerySqlCommand(command);
        }

        public static object GetUserBalance(string userid) {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "SELECT balance FROM users WHERE userid==@userid";
            command.Prepare();
            command.Parameters.AddWithValue("@userid", userid);
            var balance = SqlHelper.ExcecuteScalarSqlCommand(command);
            return balance;
        }

        public static bool UpdateUserBalance(string userid, decimal balance) {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "UPDATE users SET balance = @balance WHERE userid==@userid";
            command.Prepare();
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@userid", userid);
            return SqlHelper.ExcecuteNonQuerySqlCommand(command);
        }
    }
}