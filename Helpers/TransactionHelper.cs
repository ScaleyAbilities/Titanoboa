using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
namespace Titanoboa
{
    public static class TransactionHelper
    {
        public static bool AddTransaction(string userid, decimal balance, string stockSymbol, string commandText, decimal balanceChange, int stockAmount, bool pending)
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
            return SqlHelper.ExcecuteNonQuerySqlCommand(command);
        }

        public static bool AddUser(string userid, decimal balance) 
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "INSERT INTO users (balance, userid) Values ('@balance', '@userid')";
            command.Prepare();
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@userid", userid);
            return SqlHelper.ExcecuteNonQuerySqlCommand(command);
        }

        internal static decimal GetStockPrice(string stockSymbol)
        {
            throw new NotImplementedException();
        }

        internal static Integer GetStocks(string userid, string stockSymbol)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "SELECT amount FROM stocks WHERE stocksymbol = @'stockSymbol' AND userid = @'userid'";
            command.Prepare();
            command.Parameters.AddWithValue("@stockSymbol", stockSymbol);
            command.Parameters.AddWithValue("@userid", userid);
            return SqlHelper.ExcecuteScalarSqlCommand(command);
        }

        // Method to output json object of all transactions or tansactions for single user.
        internal static JObject GetTransactions(string userid, bool isAdmin)
        {
            throw new NotImplementedException();
        }

        public static object GetUserBalance(string userid) 
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
            var balance = SqlHelper.ExcecuteScalarSqlCommand(command);
            return balance;
        }

        public static bool IsAdmin(string userid)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = @"
                SELECT CASE WHEN EXISTS(
                    SELECT * FROM users WHERE 
                    userid = @userid AND 
                    isadmin
                )
                THEN CAST(1 AS BIT)
                ELSE CAST(0 AS BIT) END";
            command.Prepare();
            command.Parameters.AddWithValue("@userid", userid);
            return SqlHelper.ExcecuteScalarSqlCommand(command);
        }

        public static bool UpdateUserBalance(string userid, decimal balance) 
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "UPDATE users SET balance = @balance WHERE userid=@userid";
            command.Prepare();
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@userid", userid);
            return SqlHelper.ExcecuteNonQuerySqlCommand(command);
        }
    }
}