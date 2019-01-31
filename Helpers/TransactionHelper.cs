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

        public static void AddUser(string userid, decimal balance) 
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "INSERT INTO users (balance, userid) Values ('@balance', '@userid')";
            command.Prepare();
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@userid", userid);
            command.ExecuteNonQuery();
        }

        internal static decimal GetStockPrice(string stockSymbol)
        {
            // TODO: this
            return 1;
        }

        public static int GetStocks(string userid, string stockSymbol)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "SELECT amount FROM stocks WHERE stocksymbol = @'stockSymbol' AND userid = @'userid'";
            command.Prepare();
            command.Parameters.AddWithValue("@stockSymbol", stockSymbol);
            command.Parameters.AddWithValue("@userid", userid);
            return (int?)command.ExecuteScalar() ?? 0;
        }

        public static decimal? GetUserBalance(string userid) 
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