using System;

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

        private ulong workId;

        public Logger()
        {
            // Create new unit of work and get ID
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs_work VALUES (); SELECT LAST_INSERT_ID();";
            sqlCommand.Prepare();
            workId = Convert.ToUInt64(sqlCommand.ExecuteScalar());
        }

        public void LogCommand(string command, User user = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, server, workid, command, userid, amount, stocksymbol, filename)
                                       VALUES ('command', @server, @workid, @command, @userid, @amount, @stocksymbol, @filename)";
            sqlCommand.Parameters.AddWithValue("@server", Program.ServerName);
            sqlCommand.Parameters.AddWithValue("@workid", workId);
            sqlCommand.Parameters.AddWithValue("@command", command);
            sqlCommand.Parameters.AddWithValue("@userid", user?.Id);
            sqlCommand.Parameters.AddWithValue("@amount", amount);
            sqlCommand.Parameters.AddWithValue("@stocksymbol", stockSymbol);
            sqlCommand.Parameters.AddWithValue("@filename", filename);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();
        }

        public void LogQuoteServer(User user, decimal? amount, string stockSymbol, DateTime quoteServerTime, string cryptoKey)
        {
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, server, workid, userid, amount, stocksymbol, quoteservertime, cryptokey)
                                       VALUES ('command', @server, @workid, @userid, @amount, @stocksymbol, @quoteservertime, @cryptokey)";
            sqlCommand.Parameters.AddWithValue("@server", Program.ServerName);
            sqlCommand.Parameters.AddWithValue("@workid", workId);
            sqlCommand.Parameters.AddWithValue("@userid", user.Id);
            sqlCommand.Parameters.AddWithValue("@amount", amount);
            sqlCommand.Parameters.AddWithValue("@stocksymbol", stockSymbol);
            sqlCommand.Parameters.AddWithValue("@quoteservertime", quoteServerTime);
            sqlCommand.Parameters.AddWithValue("@cryptokey", cryptoKey);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();
        }

        public void LogEvent(EventType type, string message, User user = null, decimal? amount = null, string stockSymbol = null, string command = null, string filename = null)
        {
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, server, workid, message, userid, amount, stocksymbol, command, filename)
                                       VALUES (@type, @server, @workid, @message, @userid, @amount, @stocksymbol, @command, @filename)";
            sqlCommand.Parameters.AddWithValue("@type", type.ToString("F").ToLower());
            sqlCommand.Parameters.AddWithValue("@server", Program.ServerName);
            sqlCommand.Parameters.AddWithValue("@workid", workId);
            sqlCommand.Parameters.AddWithValue("@message", message);
            sqlCommand.Parameters.AddWithValue("@userid", user?.Id);
            sqlCommand.Parameters.AddWithValue("@amount", amount);
            sqlCommand.Parameters.AddWithValue("@stocksymbol", stockSymbol);
            sqlCommand.Parameters.AddWithValue("@command", command);
            sqlCommand.Parameters.AddWithValue("@filename", filename);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();
        }

        public void LogTransaction(Transaction transaction)
        {
            string message;
            if (transaction.Type == "trigger")
                message = $"{transaction.Command} (trigger - {transaction.StockAmount} stocks at ${transaction.StockPrice})";
            else
                message = $"{transaction.Command} ({transaction.Type})";

            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, server, workid, message, userid, amount)
                                       VALUES ('transaction', @server, @workid, @message, @userid, @amount)";
            sqlCommand.Parameters.AddWithValue("@server", Program.ServerName);
            sqlCommand.Parameters.AddWithValue("@workid", workId);
            sqlCommand.Parameters.AddWithValue("@message", message);
            sqlCommand.Parameters.AddWithValue("@userid", transaction.User.Id);
            sqlCommand.Parameters.AddWithValue("@amount", transaction.BalanceChange);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();
        }
    }
}