using System;

namespace Titanoboa
{
    public static class LogHelper
    {
        public enum EventType
        {
            System,
            Error,
            Debug
        }

        public static void LogCommand(Transaction transaction, string filename = null)
        {
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, timestamp, server, transactionid, command, userid, stocksymbol, filename, funds)
                                       VALUES ('command', @curTime, @server, @transactionId, @filename)";
            sqlCommand.Parameters.AddWithValue("@curTime", DateTime.Now);
            sqlCommand.Parameters.AddWithValue("@server", Program.ServerName);
            sqlCommand.Parameters.AddWithValue("@transactionId", transaction.Id);
            sqlCommand.Parameters.AddWithValue("@filename", filename);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();

            LogTransaction(transaction);
        }

        public static void LogQuoteServer(Transaction transaction, DateTime quoteServerTime, string cryptoKey)
        {
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, timestamp, server, transactionid, quoteservertime, cryptokey)
                                       VALUES ('quote', @curTime, @server, @transactionId, @quoteservertime, @cryptokey)";
            sqlCommand.Parameters.AddWithValue("@curTime", DateTime.Now);
            sqlCommand.Parameters.AddWithValue("@server", Program.ServerName);
            sqlCommand.Parameters.AddWithValue("@transactionId", transaction.Id);
            sqlCommand.Parameters.AddWithValue("@quoteservertime", quoteServerTime);
            sqlCommand.Parameters.AddWithValue("@cryptokey", cryptoKey);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();

            LogTransaction(transaction);
        }

        public static void LogEvent(EventType type, Transaction transaction, string message, string filename = null)
        {
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, timestamp, server, transactionid, message, filename)
                                       VALUES (@type, @curTime, @server, @transactionId, @message, @filename)";
            sqlCommand.Parameters.AddWithValue("@type", type.ToString("F").ToLower());
            sqlCommand.Parameters.AddWithValue("@curTime", DateTime.Now);
            sqlCommand.Parameters.AddWithValue("@server", Program.ServerName);
            sqlCommand.Parameters.AddWithValue("@transactionId", transaction.Id);
            sqlCommand.Parameters.AddWithValue("@message", message);
            sqlCommand.Parameters.AddWithValue("@filename", filename);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();

            LogTransaction(transaction);
        }

        private static void LogTransaction(Transaction transaction)
        {
            if (transaction.HasBeenLogged)
                return;

            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, timestamp, server, transactionid)
                                       VALUES ('transaction', @curTime, @server, @transactionId)";
            sqlCommand.Parameters.AddWithValue("@curTime", DateTime.Now);
            sqlCommand.Parameters.AddWithValue("@server", Program.ServerName);
            sqlCommand.Parameters.AddWithValue("@transactionId", transaction.Id);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();

            transaction.HasBeenLogged = true;
        }
    }
}