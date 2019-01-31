using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class TransactionHelper
    {
        public static void AddTransaction(string userid, decimal balance, string stockSymbol, string commandText, decimal balanceChange, int stockAmount, bool pending)
        {
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
            command.ExecuteNonQuery();
        }

        internal static decimal GetStockPrice(string stockSymbol)
        {
            throw new NotImplementedException();
        }

        public static void AddUser(string userid, decimal balance) 
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "INSERT INTO users (balance, userid) Values ('@balance', '@userid')";
            command.Prepare();
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@userid", userid);
            command.ExecuteNonQuery();
        }

        public static decimal? GetUserPendingBalance(string userid) 
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();

            //Get balance minus pending transactions balance changes
            command.CommandText = @"SELECT balance + SUM(IFNULL(transactions.balancechange, 0))  
                                FROM users LEFT JOIN transactions ON users.id = transactions.usernum 
                                AND transactions.transactiontime >= DATE_SUB(@curTime, INTERVAL 60 SECOND)  
                                AND transactions.command = ""BUY"" 
                                AND transactions.pendingflag = 1
                                WHERE users.userid = @userid";
            command.Prepare();
            command.Parameters.AddWithValue("@curTime", DateTime.Now);
            command.Parameters.AddWithValue("@userid", userid);
            var balance = (decimal?)command.ExecuteScalar();
            return balance;
        }

        public static decimal? GetUserActualBalance(string userid) 
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();

            command.CommandText = "SELECT balance FROM users WHERE user.id = @userid";
            command.Prepare();
            command.Parameters.AddWithValue("@userid", userid);
            var balance = (decimal?)command.ExecuteScalar();
            return balance;
        }

        public static MySqlDataReader GetMostRecentTransaction(string userid, string commandText, int pending) {
            MySqlCommand command = SqlHelper.CreateSqlCommand();

            command.CommandText = @"SELECT * FROM transactions WHERE transactions.userid = @userid 
                                AND transactions.transactiontime >= DATE_SUB(@curTime, INTERVAL 60 SECOND)
                                AND transactions.command = @commandText
                                AND transactions.pendingflag = @pending
                                ORDER BY transactions.transactiontime DESC
                                LIMIT 1";
            command.Prepare();
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@curTime", DateTime.Now);
            command.Parameters.AddWithValue("@commandText", commandText);
            command.Parameters.AddWithValue("@pending", pending);
            MySqlDataReader mostRecentTransaction = command.ExecuteReader();
            return mostRecentTransaction;
        }

        public static void UpdateUserBalance(string userid, decimal balance) 
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "UPDATE users SET balance = @balance WHERE userid=@userid";
            command.Prepare();
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@userid", userid);
            command.ExecuteNonQuery();
        }
    }
}