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

        private static string sqlHost = Environment.GetEnvironmentVariable("SQL_HOST") ?? "localhost";
        private static string connectionString = $"Server={sqlHost};Database=scaley_abilities;Uid=scaley;Pwd=abilities;";
        private static NpgsqlConnection connection = new NpgsqlConnection(connectionString);
        private static Mutex sqlMutex = new Mutex();


        private ulong workId;
        private NpgsqlCommand fullCommand = new NpgsqlCommand();

        // We use a StringBuilder to build up the command since it's faster than using += on a string
        private StringBuilder fullCommandText = new StringBuilder();
        private int insertNum = 0;

        static Logger()
        {
            connection.Open();
        }
        
        public Logger()
        {
            // Create new unit of work and get ID
            fullCommandText.Append(@"WITH workid (workid) AS (INSERT INTO logs_work DEFAULT VALUES RETURNING id)");
            fullCommand.Parameters.AddWithValue($"@server", Program.ServerName);
            fullCommand.Parameters.AddWithValue($"@command", Program.CurrentCommand);
        }

        public void LogCommand(User user = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            fullCommandText.Append($@", i{insertNum} AS (INSERT INTO logs (logtype, server, workid, command, userid, amount, stocksymbol, filename)
                                      SELECT 'command'::log_type, @server, workid, @command, @userid{insertNum}, @amount{insertNum}, @stocksymbol{insertNum}, @filename{insertNum}
                                      FROM workid)");
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", (object)user?.Id ?? DBNull.Value);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", (object)amount ?? DBNull.Value);
            fullCommand.Parameters.AddWithValue($"@stocksymbol{insertNum}", (object)stockSymbol ?? DBNull.Value);
            fullCommand.Parameters.AddWithValue($"@filename{insertNum}", (object)filename ?? DBNull.Value);

            insertNum++;
        }

        public void LogQuoteServer(User user, decimal amount, string stockSymbol, DateTime quoteServerTime, string cryptoKey)
        {
            fullCommandText.Append($@", i{insertNum} AS (INSERT INTO logs (logtype, server, workid, userid, amount, stocksymbol, quoteservertime, cryptokey)
                                      SELECT 'quote'::log_type, @server, workid, @userid{insertNum}, @amount{insertNum}, @stocksymbol{insertNum}, @quoteservertime{insertNum}, @cryptokey{insertNum}
                                      FROM workid)");
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", user.Id);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", amount);
            fullCommand.Parameters.AddWithValue($"@stocksymbol{insertNum}", stockSymbol);
            fullCommand.Parameters.AddWithValue($"@quoteservertime{insertNum}", quoteServerTime);
            fullCommand.Parameters.AddWithValue($"@cryptokey{insertNum}", cryptoKey);

            insertNum++;
        }

        public void LogEvent(EventType type, string message, User user = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            fullCommandText.Append($@", i{insertNum} AS (INSERT INTO logs (logtype, server, workid, command, message, userid, amount, stocksymbol, filename)
                                      SELECT @type{insertNum}::log_type, @server, workid, @command, @message{insertNum}, @userid{insertNum}, @amount{insertNum}, @stocksymbol{insertNum}, @filename{insertNum}
                                      FROM workid)");
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

            fullCommandText.Append($@", i{insertNum} AS (INSERT INTO logs (logtype, server, workid, message, userid, amount)
                                      SELECT 'transaction'::log_type, @server, workid, @message{insertNum}, @userid{insertNum}, @amount{insertNum}
                                      FROM workid)");
            fullCommand.Parameters.AddWithValue($"@message{insertNum}", message);
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", transaction.User.Id);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", transaction.BalanceChange);

            insertNum++;
        }

        public void CommitLogs()
        {
            if (insertNum <= 0)
                return;

            fullCommandText.Append("SELECT NULL;");

            var thread = new Thread(() => {
                fullCommand.Connection = connection;
                fullCommand.CommandText = fullCommandText.ToString();

                sqlMutex.WaitOne();
                fullCommand.Prepare();
                fullCommand.ExecuteNonQuery();
                sqlMutex.ReleaseMutex();
            });
            thread.Start();
        }
    }
}
