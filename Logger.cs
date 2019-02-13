using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

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
        private static string connectionString = $"Server={sqlHost};Database=scaley_abilities;Uid=scaley;Pwd=abilities;Allow User Variables=True";
        private static MySqlConnection connection = new MySqlConnection(connectionString);
        private static Mutex sqlMutex = new Mutex();

        private static int runningThreads = 0;


        private ulong workId;
        private MySqlCommand fullCommand = new MySqlCommand();

        // We use a StringBuilder to build up the command since it's faster than using += on a string
        private StringBuilder fullCommandText = new StringBuilder();
        private int insertNum = 0;
        private bool committed = false;

        static Logger()
        {
            connection.Open();
        }
        
        public Logger()
        {
            // Create new unit of work and get ID
            fullCommandText.Append(@"INSERT INTO logs_work VALUES (); SELECT @workid := LAST_INSERT_ID();");
            fullCommand.Parameters.AddWithValue($"@server", Program.ServerName);
            fullCommand.Parameters.AddWithValue($"@command", Program.CurrentCommand);
        }

        public void LogCommand(User user = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            fullCommandText.Append($@"INSERT INTO logs (logtype, server, workid, command, userid, amount, stocksymbol, filename)
                                      VALUES ('command', @server, @workid, @command, @userid{insertNum},
                                              @amount{insertNum}, @stocksymbol{insertNum}, @filename{insertNum});");
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", user?.Id);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", amount);
            fullCommand.Parameters.AddWithValue($"@stocksymbol{insertNum}", stockSymbol);
            fullCommand.Parameters.AddWithValue($"@filename{insertNum}", filename);

            insertNum++;
        }

        public void LogQuoteServer(User user, decimal? amount, string quoteStockSymbol, string quoteUserId, string quoteServerTime, string cryptoKey)
        {
            fullCommandText.Append($@"INSERT INTO logs (logtype, server, workid, userid, amount, stocksymbol, message, quoteservertime, cryptokey)
                                      VALUES ('quote', @server, @workid, @userid{insertNum}, @amount{insertNum},
                                              @stocksymbol{insertNum}, @quoteuser{insertNum}, @quoteservertime{insertNum}, @cryptokey{insertNum});");
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", user.Id);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", amount);
            fullCommand.Parameters.AddWithValue($"@stocksymbol{insertNum}", quoteStockSymbol);
            fullCommand.Parameters.AddWithValue($"@quoteuser{insertNum}", quoteUserId);
            fullCommand.Parameters.AddWithValue($"@quoteservertime{insertNum}", quoteServerTime);
            fullCommand.Parameters.AddWithValue($"@cryptokey{insertNum}", cryptoKey);

            insertNum++;
        }

        public void LogEvent(EventType type, string message, User user = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            fullCommandText.Append($@"INSERT INTO logs (logtype, server, workid, command, message, userid, amount, stocksymbol, filename)
                                      VALUES (@type{insertNum}, @server, @workid, @command, @message{insertNum},
                                      @userid{insertNum}, @amount{insertNum}, @stocksymbol{insertNum}, @filename{insertNum});");
            fullCommand.Parameters.AddWithValue($"@type{insertNum}", type.ToString("F").ToLower());
            fullCommand.Parameters.AddWithValue($"@message{insertNum}", message);
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", user?.Id);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", amount);
            fullCommand.Parameters.AddWithValue($"@stocksymbol{insertNum}", stockSymbol);
            fullCommand.Parameters.AddWithValue($"@filename{insertNum}", filename);

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
                                      VALUES ('transaction', @server, @workid, @message{insertNum}, 
                                      @userid{insertNum}, @amount{insertNum});");
            fullCommand.Parameters.AddWithValue($"@message{insertNum}", message);
            fullCommand.Parameters.AddWithValue($"@userid{insertNum}", transaction.User.Id);
            fullCommand.Parameters.AddWithValue($"@amount{insertNum}", transaction.BalanceChange);

            insertNum++;
        }

        public void CommitLogs()
        {
            if (insertNum <= 0 || committed)
                return;

            Thread thread = null;
            thread = new Thread(() => {
                fullCommand.Connection = connection;
                fullCommand.CommandText = fullCommandText.ToString();
                fullCommand.Prepare();

                sqlMutex.WaitOne();
                fullCommand.ExecuteNonQuery();
                runningThreads--;
                sqlMutex.ReleaseMutex();
            });
            runningThreads++;
            thread.Start();

            committed = true;
        }

        public static void WaitForTasks()
        {
            while (runningThreads > 0)
            {
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
