using System;
using System.Data;
using System.Data.Common;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Titanoboa
{
    public class DatabaseHelper : IDisposable
    {
        private DbConnection connection;
        private DbTransaction transaction;
        private Logger logger;

        public DatabaseHelper(DbConnection connection, Logger logger)
        {
            this.connection = connection;
            this.transaction = this.connection.BeginTransaction();
            this.logger = logger;
        }

        public User GetUser(string username, bool withPendingBalance = false) 
        {
            using (var command = GetCommand())
            {
                if (!withPendingBalance)
                {
                    command.CommandText = @"SELECT * FROM users WHERE username = @username";
                }
                else
                {
                    command.CommandText = 
                        @"SELECT users.*, balance + COALESCE(pending_sum, 0) AS pending_balance
                        FROM users LEFT JOIN (
                            SELECT userid, SUM(balancechange) AS pending_sum FROM transactions
                            WHERE transactiontime >= NOW() - INTERVAL '60' SECOND AND command = 'BUY' AND type = 'pending'
                            GROUP BY userid
                        ) t ON t.userid = users.id
                        WHERE users.username = @username";
                }
                
                command.Parameters.AddWithValue("@username", username);
                command.Prepare();
                
                var reader = command.ExecuteReader();
                var createdNewUser = false;
                if (!reader.HasRows)
                {
                    // User doesn't exist, create them
                    reader.Close();

                    using (var createCommand = GetCommand())
                    {
                        createCommand.CommandText = "INSERT INTO users (username, balance) VALUES (@username, 0)";
                        createCommand.Parameters.AddWithValue("@username", username);
                        createCommand.Prepare();
                        createCommand.ExecuteNonQuery();
                    }                    

                    createdNewUser = true;

                    // Re-run the get user command
                    reader = command.ExecuteReader();
                }

                // Use try-finally since we couldn't use a using block
                User user;
                try
                {
                    reader.Read();

                    user = new User() {
                        Id = Convert.ToInt64(reader["id"]),
                        Username = (string)reader["username"],
                        Balance = (decimal)reader["balance"]
                    };

                    if (withPendingBalance)
                        user.PendingBalance = (decimal)reader["pending_balance"];
                }
                finally
                {
                    reader.Close();
                }

                if (createdNewUser)
                    logger.LogEvent(Logger.EventType.Debug, $"Created new user", user);
                
                return user;
            }
        }

        public void UpdateUserBalance(ref User user, decimal balance)
        {
            using (var command = GetCommand())
            {
                command.CommandText = "UPDATE users SET balance = @balance WHERE id = @userid";
                command.Parameters.AddWithValue("@balance", balance);
                command.Parameters.AddWithValue("@userid", user.Id);
                command.Prepare();
                command.ExecuteNonQuery();
                
                // Now that the balance has been updated, update the model
                user.Balance = balance;

                logger.LogEvent(Logger.EventType.Debug, $"Updated user balance", user, balance);
            }
        }

        public Transaction CreateTransaction(
            User user, 
            string stockSymbol, 
            string commandText, 
            decimal balanceChange = 0.00m,
            int? stockAmount = null, 
            decimal? stockPrice = null, 
            string type = "completed"
        ) {
            using (var command = GetCommand())
            {
                command.CommandText = @"INSERT INTO transactions (userid, stocksymbol, command, balancechange, stockamount, stockprice, type) 
                                        values (@userid, @stocksymbol, @command, @balancechange, @stockamount, @stockprice, @type::txn_type)
                                        RETURNING id;";

                command.Parameters.AddWithValue("@userid", user.Id);
                command.Parameters.AddWithValue("@stocksymbol", (object)stockSymbol ?? DBNull.Value);
                command.Parameters.AddWithValue("@command", commandText);
                command.Parameters.AddWithValue("@balancechange", balanceChange);
                command.Parameters.AddWithValue("@stockamount", (object)stockAmount ?? DBNull.Value);
                command.Parameters.AddWithValue("@stockprice", (object)stockPrice ?? DBNull.Value);
                command.Parameters.AddWithValue("@type", type);
                command.Prepare();
                var id = Convert.ToInt64(command.ExecuteScalar());

                var transaction = new Transaction() {
                    Id = id,
                    User = user,
                    Command = commandText,
                    BalanceChange = balanceChange,
                    StockSymbol = stockSymbol,
                    StockAmount = stockAmount,
                    StockPrice = stockPrice,
                    Type = type
                };

                logger.LogTransaction(transaction);
                
                return transaction;
            }
        }

        public Transaction GetLatestPendingTransaction(User user, string commandText) {
            using (var command = GetCommand())
            {
                command.CommandText = @"SELECT id, command, balancechange, stocksymbol, stockamount, stockprice, type
                                        FROM transactions WHERE userid = @userid
                                        AND transactiontime >= NOW() - INTERVAL '60' SECOND
                                        AND command = @commandText
                                        AND type = 'pending'
                                        ORDER BY transactiontime DESC
                                        LIMIT 1";

                command.Parameters.AddWithValue("@userid", user.Id);
                command.Parameters.AddWithValue("@commandText", commandText);
                command.Prepare();

                Transaction transaction = null;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        transaction = new Transaction() {
                            Id = Convert.ToInt64(reader["id"]),
                            User = user,
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
        }

        public void CommitTransaction(ref Transaction transaction)
        {
            using (var command = GetCommand())
            {
                command.CommandText = @"UPDATE transactions SET type = 'completed' WHERE id = @id";
                command.Parameters.AddWithValue("@id", transaction.Id);
                command.Prepare();
                command.ExecuteNonQuery();

                transaction.Type = "completed";

                logger.LogTransaction(transaction);
            }
        }

        public decimal GetStockPrice(User user, string stockSymbol)
        {
            return QuoteHelper.GetQuote(user, stockSymbol);
        }

        public Transaction GetTriggerTransaction(User user, string stockSymbol, string triggerType)
        {
            using (var command = GetCommand())
            {
                command.CommandText = @"SELECT id, command, balancechange, stocksymbol, stockamount, stockprice, type
                                        FROM transactions WHERE userid = @userid
                                        AND command = @commandText
                                        AND type = 'trigger'
                                        LIMIT 1";
                
                command.Parameters.AddWithValue("@userid", user.Id);
                command.Parameters.AddWithValue("@commandText", triggerType);
                command.Prepare();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        var transaction = new Transaction() {
                            Id = Convert.ToInt64(reader["id"]),
                            User = user,
                            Command = (string)reader["command"],
                            BalanceChange = (decimal)reader["balancechange"],
                            StockSymbol = (string)reader["stocksymbol"],
                            StockAmount = SqlHelper.ConvertToNullableInt32(reader["stockamount"]),
                            StockPrice = SqlHelper.ConvertToNullableDecimal(reader["stockprice"]),
                            Type = (string)reader["type"]
                        };

                        return transaction;
                    }
                }
            }

            return null;
        }

        public void SetTransactionBalanceChange(ref Transaction transaction, decimal balanceChange)
        {
            using (var command = GetCommand())
            {
                command.CommandText = @"UPDATE transactions SET
                                        balancechange = @balanceChange
                                        WHERE id = @id";

                command.Parameters.AddWithValue("@balanceChange", balanceChange);
                command.Parameters.AddWithValue("@id", transaction.Id);
                command.Prepare();
                command.ExecuteNonQuery();

                transaction.BalanceChange = balanceChange;

                logger.LogTransaction(transaction);
            }
        }

        public void SetTransactionStockPrice(ref Transaction transaction, decimal stockPrice)
        {
            using (var command = GetCommand())
            {
                command.CommandText = @"UPDATE transactions SET stockPrice = @stockPrice 
                                        WHERE id = @id";
                
                command.Parameters.AddWithValue("@stockPrice", stockPrice);
                command.Parameters.AddWithValue("@id", transaction.Id);
                command.Prepare();
                command.ExecuteNonQuery();

                transaction.StockPrice = stockPrice;

                logger.LogTransaction(transaction);
            }
        }

        public int GetStocks(User user, string stockSymbol, bool includePending = false)
        {
            using (var command = GetCommand())
            {
                if (!includePending)
                {
                    command.CommandText = @"SELECT amount FROM stocks WHERE stocksymbol = @stockSymbol AND userid = @userid";
                }
                else
                {
                    command.CommandText = 
                        @"SELECT stocks.amount + COALESCE(pending_sum, 0) AS pending_balance
                          FROM stocks LEFT JOIN (
                              SELECT userid, SUM(stockamount) AS pending_sum FROM transactions
                              WHERE transactiontime >= NOW() - INTERVAL '60' SECOND
                              AND command = 'SELL' AND type = 'pending' AND stocksymbol = @stockSymbol
                              GROUP BY userid
                          ) t ON t.userid = stocks.userid
                          WHERE stocks.userid = @userid AND stocksymbol = @stockSymbol";
                }
                command.Parameters.AddWithValue("@stockSymbol", stockSymbol);
                command.Parameters.AddWithValue("@userid", user.Id);
                command.Prepare();

                var stocks = SqlHelper.ConvertToNullableInt32(command.ExecuteScalar());

                if (stocks == null)
                {
                    // User stocks entry doesn't exist, create with 0 stocks
                    using (var createCommand = GetCommand())
                    {
                        createCommand.CommandText = @"INSERT INTO stocks (userid, stocksymbol, amount)
                                                      VALUES (@userid, @stockSymbol, 0)";
                        createCommand.Parameters.AddWithValue("@userid", user.Id);
                        createCommand.Parameters.AddWithValue("@stockSymbol", stockSymbol);
                        createCommand.Prepare();
                        createCommand.ExecuteNonQuery();
                    }
                    
                    logger.LogEvent(Logger.EventType.Debug, $"Stocks entry not found, created new one with 0 stocks", user, null, stockSymbol);
                }

                return stocks ?? 0;
            }
        }

        public void UpdateStocks(User user, string stockSymbol, int newAmount)
        {
            using (var command = GetCommand())
            {
                command.CommandText = @"UPDATE stocks SET amount = @newAmount
                                        WHERE userid = @userid AND stocksymbol = @stockSymbol";
                command.Parameters.AddWithValue("@userid", user.Id);
                command.Parameters.AddWithValue("@stockSymbol", stockSymbol);
                command.Parameters.AddWithValue("@newAmount", newAmount);
                command.Prepare();
                command.ExecuteNonQuery();

                logger.LogEvent(Logger.EventType.Debug, $"Updated user's stocks to {newAmount}", user, null, stockSymbol);
            }            
        }

        public void DeleteTransaction(Transaction transaction) {
            using (var command = GetCommand())
            {
                command.CommandText = @"DELETE FROM transactions WHERE id = @transactionId";
                command.Parameters.AddWithValue("@transactionId", transaction.Id);
                command.Prepare();
                command.ExecuteNonQuery();

                logger.LogEvent(Logger.EventType.Debug, $"Cancelled {transaction.Command} transaction", transaction.User, transaction.BalanceChange, transaction.StockSymbol);
            }
        }

        public void CommitAllChanges()
        {
            transaction.Commit();
        }

        public void RollbackAllChanges()
        {
            transaction.Rollback();
        }
        
        private NpgsqlCommand GetCommand() {
            return SqlHelper.GetCommand(connection, transaction);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    transaction.Dispose();
                    connection.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
