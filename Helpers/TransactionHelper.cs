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
                                        AND transactions.transactiontime >= DATE_SUB(NOW(), INTERVAL 60 SECOND)
                                        AND transactions.command = ""BUY""
                                        AND transactions.type = ""pending""
                                        WHERE users.username = @username";
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
                Id = Convert.ToUInt64(reader["id"]),
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

        public static Transaction CreateTransaction(
            User user, 
            string stockSymbol, 
            string commandText, 
            decimal balanceChange = 0.00m,
            int? stockAmount = null, 
            decimal? stockPrice = null, 
            string type = "completed"
        ) {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            command.Prepare();
            command.CommandText = @"INSERT INTO transactions (userid, stocksymbol, command, balancechange, stockamount, stockprice, type) 
                                    values (@userid, @stocksymbol, @command, @balancechange, @stockamount, @stockprice, @type);
                                    SELECT LAST_INSERT_ID();";

            command.Parameters.AddWithValue("@userid", user.Id);
            command.Parameters.AddWithValue("@stocksymbol", stockSymbol);
            command.Parameters.AddWithValue("@command", commandText);
            command.Parameters.AddWithValue("@balancechange", balanceChange);
            command.Parameters.AddWithValue("@stockamount", stockAmount);
            command.Parameters.AddWithValue("@stockprice", stockPrice);
            command.Parameters.AddWithValue("@type", type);
            var id = Convert.ToUInt64(command.ExecuteScalar());

            return new Transaction() {
                Id = id,
                Command = commandText,
                BalanceChange = balanceChange,
                StockSymbol = stockSymbol,
                StockAmount = stockAmount,
                StockPrice = stockPrice,
                Type = "completed"
            };
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

            command.CommandText = @"SELECT id, command, balancechange, stocksymbol, stockamount, stockprice, type
                                    FROM transactions WHERE transactions.userid = @userid
                                    AND transactions.transactiontime >= DATE_SUB(NOW(), INTERVAL 60 SECOND)
                                    AND transactions.command = @commandText
                                    AND transactions.type = 'pending'
                                    ORDER BY transactions.transactiontime DESC
                                    LIMIT 1";
            
            command.Prepare();
            command.Parameters.AddWithValue("@userid", user.Id);
            command.Parameters.AddWithValue("@commandText", commandText);

            Transaction transaction = null;
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    transaction = new Transaction() {
                        Id = Convert.ToUInt64(reader["id"]),
                        Command = (string)reader["command"],
                        BalanceChange = (decimal)reader["balancechange"],
                        StockSymbol = (string)reader["stocksymbol"],
                        StockAmount = SqlHelper.ConvertToNullableInt32(reader["stockamount"]),
                        StockPrice = (decimal)reader["stockprice"],
                        Type = (string)reader["type"]
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

        public static void CommitTransaction(ref Transaction transaction)
        {
            var command = SqlHelper.CreateSqlCommand();

            command.CommandText = @"UPDATE transactions SET type = 'completed' WHERE id = @id";
            command.Parameters.AddWithValue("@id", transaction.Id);
            command.Prepare();
            command.ExecuteNonQuery();

            transaction.Type = "completed";
        }

        internal static decimal GetStockPrice(User user, string stockSymbol)
        {
            // TODO: Implement actual quote server
            var price = 1.00m;
            Program.Logger.LogQuoteServer(user, price, stockSymbol, DateTime.Now, "i'm a crypto key woohoo");
            return price;
        }

        public static Transaction GetTriggerTransaction(User user, string stockSymbol, string triggerType)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();
            
            command.CommandText = @"SELECT id, command, balancechange, stocksymbol, stockamount, stockprice, type
                                    FROM transactions WHERE transactions.userid = @userid
                                    AND transactions.command = @commandText
                                    AND transactions.type = 'trigger'
                                    LIMIT 1";
            
            command.Prepare();
            command.Parameters.AddWithValue("@userid", user.Id);
            command.Parameters.AddWithValue("@commandText", triggerType);

            Transaction transaction = null;
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    transaction = new Transaction() {
                        Id = Convert.ToUInt64(reader["id"]),
                        Command = (string)reader["command"],
                        BalanceChange = (decimal)reader["balancechange"],
                        StockSymbol = (string)reader["stocksymbol"],
                        StockAmount = SqlHelper.ConvertToNullableInt32(reader["stockamount"]),
                        StockPrice = SqlHelper.ConvertToNullableDecimal(reader["stockprice"]),
                        Type = (string)reader["type"]
                    };
                }
            }

            return transaction;
        }

        public static void SetTransactionBalanceChange(ref Transaction transaction, decimal balanceChange)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();

            command.CommandText = @"UPDATE transactions SET
                                    transactions.balancechange = @balanceChange
                                    WHERE transactions.id = @id";

            command.Parameters.AddWithValue("@balanceChange", balanceChange);
            command.Parameters.AddWithValue("@id", transaction.Id);
            command.Prepare();
            command.ExecuteNonQuery();

            transaction.BalanceChange = balanceChange;
        }

        public static void SetTransactionStockPrice(ref Transaction transaction, decimal stockPrice)
        {
            MySqlCommand command = SqlHelper.CreateSqlCommand();

            command.CommandText = @"UPDATE transactions SET stockPrice = @stockPrice 
                                    WHERE transactions.id = @id";
            
            command.Parameters.AddWithValue("@stockPrice", stockPrice);
            command.Parameters.AddWithValue("@id", transaction.Id);
            command.Prepare();
            command.ExecuteNonQuery();

            transaction.StockPrice = stockPrice;
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
                                        AND transactions.command = 'SELL'
                                        AND transactions.type = 'pending'
                                        AND transactions.stocksymbol = @stockSymbol
                                        WHERE users.username = @username";
            }
            command.Prepare();
            command.Parameters.AddWithValue("@stockSymbol", stockSymbol);
            command.Parameters.AddWithValue("@userid", user.Id);

            var stocks = SqlHelper.ConvertToNullableInt32(command.ExecuteScalar());
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
