using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class TransactionHelper
    {
        public static User GetUser(string username, bool withPendingBalance = false) 
        {
            var command = SqlHelper.CreateSqlCommand();

            if (!withPendingBalance)
            {
                command.CommandText = @"SELECT * FROM users WHERE username = @username";
            }
            else
            {
                command.CommandText = @"SELECT users.*, balance + SUM(IFNULL(transactions.balancechange, 0)) AS pending_balance
                                        FROM users LEFT JOIN transactions ON users.id = transactions.userid
                                        AND transactions.transactiontime >= DATE_SUB(@curTime, INTERVAL 60 SECOND)
                                        AND transactions.command = ""BUY""
                                        AND transactions.pendingflag = 1
                                        WHERE users.username = @username";

                command.Parameters.AddWithValue("@curTime", DateTime.Now);
            }
            
            command.Parameters.AddWithValue("@username", username);
            command.Prepare();
            var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
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

            if (withPendingBalance)
            {
                user.PendingBalance = (decimal)reader["pending_balance"];
            }

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

        public static void AddTransaction(User user, string stockSymbol, string commandText, decimal balanceChange, int stockAmount, bool pending)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.Prepare();
            command.CommandText = @"INSERT INTO transactions (userid, stocksymbol, command, balancechange, stockamount, pendingflag, transactiontime) 
                                    values (@userid, @stocksymbol, @command, @balancechange, @stockamount, @pending, @curTime)";
            command.Parameters.AddWithValue("@userid", user.Id);
            command.Parameters.AddWithValue("@stocksymbol", stockSymbol);
            command.Parameters.AddWithValue("@command", commandText);
            command.Parameters.AddWithValue("@balancechange", balanceChange);
            command.Parameters.AddWithValue("@stockamount", stockAmount);
            command.Parameters.AddWithValue("@pending", pending);
            command.Parameters.AddWithValue("@curTime", DateTime.Now);
            command.ExecuteNonQuery();
        }

        public static Transaction GetLatestPendingTransaction(User user, string commandText) {
            MySqlCommand command = SqlHelper.CreateSqlCommand();

            command.CommandText = @"SELECT id, balancechange, stocksymbol, stockamount FROM transactions WHERE transactions.userid = @userid
                                    AND transactions.transactiontime >= DATE_SUB(@curTime, INTERVAL 60 SECOND)
                                    AND transactions.command = @commandText
                                    AND transactions.pendingflag = 1
                                    ORDER BY transactions.transactiontime DESC
                                    LIMIT 1";
            
            command.Prepare();
            command.Parameters.AddWithValue("@userid", user.Id);
            command.Parameters.AddWithValue("@curTime", DateTime.Now);
            command.Parameters.AddWithValue("@commandText", commandText);

            Transaction transaction = null;
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    transaction = new Transaction() {
                        Id = (int)reader["id"],
                        BalanceChange = (decimal)reader["balancechange"],
                        StockSymbol = (string)reader["stocksymbol"],
                        StockAmount = (int)reader["stockamount"]
                    };
                }
            }

            return transaction;
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