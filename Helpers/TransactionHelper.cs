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

        // Method to output json object of all transactions or tansactions for single user.
        internal static JObject GetAllLogs()
        {
            throw new NotImplementedException();
        }

        // Method to output json object of all transactions or tansactions for single user.
        internal static JObject GetUserLogs(User user)
        {
            throw new NotImplementedException();
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


        public static bool IsAdmin(string userid)
        {
            // This is unused and won't work right now cuause we don't have admin info in the db

            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = @"SELECT if(isadmin) FROM users WHERE userid = @userid"; 
            command.Prepare();
            command.Parameters.AddWithValue("@userid", userid);
            return (bool)command.ExecuteScalar();
        }

        public static void CommitTransaction(Transaction transaction)
        {
            var command = SqlHelper.CreateSqlCommand();

            command.CommandText = @"UPDATE transactions SET pendingflag = 0, transactiontime = @curTime
                                    WHERE id = @id";
            command.Parameters.AddWithValue("@id", transaction.Id);
            command.Parameters.AddWithValue("@curTime", DateTime.Now);
            command.Prepare();
        }

        internal static decimal GetStockPrice(string stockSymbol)
        {
            // TODO: this
            return 1;
        }

        public static int GetStocks(User user, string stockSymbol, bool includePending = false)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();

            if(!includePending) {
                command.CommandText = @"SELECT amount FROM stocks WHERE stocksymbol = @stockSymbol AND userid = @userid";
            }
            else
            {
                command.CommandText = @"SELECT stocks.amount + SUM(IFNULL(transactions.stockamount, 0))
                                        FROM stocks LEFT JOIN transactions ON users.id = transactions.userid
                                        AND transactions.transactiontime >= DATE_SUB(@curTime, INTERVAL 60 SECOND)
                                        AND transactions.command = ""SELL""
                                        AND transactions.pendingflag = 1
                                        AND transactions.stocksymbol = @stockSymbol
                                        WHERE users.username = @username";
            }
            command.Prepare();
            command.Parameters.AddWithValue("@stockSymbol", stockSymbol);
            command.Parameters.AddWithValue("@userid", user.Id);

            var stocks = (int?)command.ExecuteScalar();

            if (stocks == null)
            {
                // User stocks entry doesn't exist, create with 0 stocks
                var createCommand = SqlHelper.CreateSqlCommand();
                createCommand.CommandText = @"INSERT INTO stocks (userid, stocksymbol, amount)
                                              VALUES (@userid, @stockSymbol, 0)";
                createCommand.Parameters.AddWithValue("@userid", user.Id);
                createCommand.Parameters.AddWithValue("@stockSymbol", stockSymbol);
                createCommand.Prepare();
                createCommand.ExecuteNonQuery();
            }

            // If it was null, then we created it and made it 0, so return 0 if null
            return stocks ?? 0;
        }

        public static void UpdateStocks(User user, string stockSymbol, int newAmount)
        {
            var command = SqlHelper.CreateSqlCommand();
            command.CommandText = @"UPDATE stocks SET amount = @newAmount
                                    WHERE userid = @userid AND stocksymbol = @stockSymbol";
            command.Parameters.AddWithValue("@userid", user.Id);
            command.Parameters.AddWithValue("@stockSymbol", stockSymbol);
            command.Parameters.AddWithValue("@newAmount", newAmount);
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public static void DeleteTransaction(Transaction transaction) {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.CommandText = @"DELETE FROM transactions WHERE transactions.id = @transactionId";
            command.Prepare();
            command.Parameters.AddWithValue("@transactionId", transaction.Id);
            command.ExecuteNonQuery();
        }
    }
}