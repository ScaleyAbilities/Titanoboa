using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class TransactionHelper
    {
        public static User GetUser(string username) 
        {
            var command = SqlHelper.CreateSqlCommand();
            command.CommandText = @"SELECT * FROM users WHERE username = @username";
            command.Parameters.AddWithValue("@username", username);
            command.Prepare();
            var reader = command.ExecuteReader();

            if (!reader.HasRows) {
                // User doesn't exist, create them
                reader.Close();
                var createCommand = SqlHelper.CreateSqlCommand();
                createCommand.CommandText = "INSERT INTO users (username, balance) VALUES (@username, 0)";
                createCommand.Parameters.AddWithValue("@username", username);
                createCommand.Prepare();
                createCommand.ExecuteNonQuery();

                // Re-run the get user command
                reader = command.ExecuteReader();
            }

            reader.Read();

            var user = new User() {
                Id = (int)reader["id"],
                Username = (string)reader["username"],
                Balance = (decimal)reader["balance"]
            };

            reader.Close();

            return user;
        }

        public static void UpdateUserBalance(ref User user, decimal balance)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = "UPDATE users SET balance = @balance WHERE id = @userid";
            command.Prepare();
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@userid", user.Id);
            command.ExecuteNonQuery();
            
            // Now that the balance has been updated, update the model
            user.Balance = balance;
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

        public static void AddTransaction(User user, string stockSymbol, string commandText, decimal balanceChange, int stockAmount, bool pending)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.Prepare();
            command.CommandText = @"INSERT INTO transactions (userid, stocksymbol, command, balancechange, stockamount, pendingflag) 
                                    values (@userid, @stocksymbol, @command, @balancechange, @stockamount, @pending)";
            command.Parameters.AddWithValue("@userid", user.Id);
            command.Parameters.AddWithValue("@stocksymbol", stockSymbol);
            command.Parameters.AddWithValue("@command", commandText);
            command.Parameters.AddWithValue("@balancechange", balanceChange);
            command.Parameters.AddWithValue("@stockamount", stockAmount);
            command.Parameters.AddWithValue("@pending", pending);
            command.ExecuteNonQuery();
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

        internal static decimal GetStockPrice(string stockSymbol)
        {
            // TODO: this
            return 1;
        }

        public static int GetStocks(User user, string stockSymbol)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = @"SELECT amount FROM stocks WHERE stocksymbol = @stockSymbol AND userid = @userid";
            command.Prepare();
            command.Parameters.AddWithValue("@stockSymbol", stockSymbol);
            command.Parameters.AddWithValue("@userid", user.Id);
            return (int?)command.ExecuteScalar() ?? 0;
        }
    }
}