using System;
using System.Text;
using System.Threading;
using Npgsql;

namespace Titanoboa
{
    public class Logger
    {
        public enum EventType
        {
            System,
            Error,
            Debug
        }

        public long WorkId;
        private NpgsqlCommand fullCommand = new NpgsqlCommand();

        // We use a StringBuilder to build up the command since it's faster than using += on a string
        private StringBuilder fullCommandText = new StringBuilder();
        private int insertNum = 0;

        public Logger()
        {
            // We need a work ID, so we'll create one in the DB
            using (var connection = SqlHelper.GetConnection())
            using (var command = SqlHelper.GetCommand(connection))
            {
                command.CommandText = "INSERT INTO logs_work DEFAULT VALUES RETURNING id";
                WorkId = (long)command.ExecuteScalar();
            }

            // Create new unit of work and get ID
            fullCommand.Parameters.AddWithValue("@workid", WorkId);
            fullCommand.Parameters.AddWithValue("@server", Program.ServerName);
            fullCommand.Parameters.AddWithValue("@command", Program.CurrentCommand);
        }

        public void LogCommand(User user = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            fullCommandText.Append($@"INSERT INTO logs (logtype, server, workid, command, userid, amount, stocksymbol, filename)
                                      VALUES ('command'::log_type, @server, @workid, @command, @userid{insertNum}, @amount{insertNum}, @stocksymbol{insertNum}, @filename{insertNum});");
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", (object)user?.Id ?? DBNull.Value);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", (object)amount ?? DBNull.Value);
            fullCommand.Parameters.AddWithValue($"@stocksymbol{insertNum}", (object)stockSymbol ?? DBNull.Value);
            fullCommand.Parameters.AddWithValue($"@filename{insertNum}", (object)filename ?? DBNull.Value);

            insertNum++;
        }

        public void LogQuoteServer(User user, decimal amount, string stockSymbol, DateTime quoteServerTime, string cryptoKey)
        {
            fullCommandText.Append($@"INSERT INTO logs (logtype, server, workid, userid, amount, stocksymbol, quoteservertime, cryptokey)
                                      VALUES ('quote'::log_type, @server, @workid, @userid{insertNum}, @amount{insertNum}, @stocksymbol{insertNum}, @quoteservertime{insertNum}, @cryptokey{insertNum});");
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", user.Id);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", amount);
            fullCommand.Parameters.AddWithValue($"@stocksymbol{insertNum}", stockSymbol);
            fullCommand.Parameters.AddWithValue($"@quoteservertime{insertNum}", quoteServerTime);
            fullCommand.Parameters.AddWithValue($"@cryptokey{insertNum}", cryptoKey);

            insertNum++;
        }

        public void LogEvent(EventType type, string message, User user = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            fullCommandText.Append($@"INSERT INTO logs (logtype, server, workid, command, message, userid, amount, stocksymbol, filename)
                                      VALUES (@type{insertNum}::log_type, @server, @workid, @command, @message{insertNum}, @userid{insertNum}, @amount{insertNum}, @stocksymbol{insertNum}, @filename{insertNum});");
            fullCommand.Parameters.AddWithValue($"@type{insertNum}", type.ToString("F").ToLower());
            fullCommand.Parameters.AddWithValue($"@message{insertNum}", message);
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", (object)user?.Id ?? DBNull.Value);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", (object)amount ?? DBNull.Value);
            fullCommand.Parameters.AddWithValue($"@stocksymbol{insertNum}", (object)stockSymbol ?? DBNull.Value);
            fullCommand.Parameters.AddWithValue($"@filename{insertNum}", (object)filename ?? DBNull.Value);

            insertNum++;
        }

        public void LogTransaction(Transaction transaction)
        {
            string message;
            if (transaction.Type == "trigger")
                message = $"{transaction.Command} (trigger, stock {transaction.StockSymbol} at ${transaction.StockPrice})";
            else
                message = $"{transaction.Command} ({transaction.Type}, stock {transaction.StockSymbol})";

            fullCommandText.Append($@"INSERT INTO logs (logtype, server, workid, message, userid, amount)
                                      VALUES ('transaction'::log_type, @server, @workid, @message{insertNum}, @userid{insertNum}, @amount{insertNum});");
            fullCommand.Parameters.AddWithValue($"@message{insertNum}", message);
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", transaction.User.Id);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", transaction.BalanceChange);

            insertNum++;
        }

        public void CommitLogs()
        {
            if (insertNum <= 0)
                return;

            var thread = new Thread(() => {
                using (var connection = SqlHelper.GetConnection())
                {
                    fullCommand.Connection = connection;
                    fullCommand.CommandText = fullCommandText.ToString();
                    fullCommand.Prepare();
                    fullCommand.ExecuteNonQuery();
                    fullCommand.Dispose();
                }
            });
            thread.Start();
        }
    }
}
