using System;

namespace Titanoboa
{
    public static class LogHelper
    {
        public static void LogCommand(
            string server,
            int transactionId,
            string command,
            int? userId = null,
            string stockSymbol = null,
            string filename = null,
            decimal? funds = null
        ) {
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, timestamp, server, transactionid, command, userid, stocksymbol, filename, funds)
                                       VALUES ('command', @curTime, @server, @transactionId, @command, @userid, @stocksymbol, @filename, @funds)";
            sqlCommand.Parameters.AddWithValue("@curTime", DateTime.Now);
            sqlCommand.Parameters.AddWithValue("@server", server);
            sqlCommand.Parameters.AddWithValue("@transactionId", transactionId);
            sqlCommand.Parameters.AddWithValue("@command", command);
            sqlCommand.Parameters.AddWithValue("@userid", userId);
            sqlCommand.Parameters.AddWithValue("@stocksymbol", stockSymbol);
            sqlCommand.Parameters.AddWithValue("@filename", filename);
            sqlCommand.Parameters.AddWithValue("@funds", funds);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();
        }

        public static void LogQuoteServer(
            string server,
            int transactionId,
            decimal price,
            string stockSymbol,
            int userId,
            DateTime quoteServerTime,
            string cryptoKey
        ) {
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, timestamp, server, transactionid, price, stocksymbol, userid, quoteservertime, cryptokey)
                                       VALUES ('quote', @curTime, @server, @transactionId, @price, @stocksymbol, @userid, @quoteservertime, @cryptokey)";
            sqlCommand.Parameters.AddWithValue("@curTime", DateTime.Now);
            sqlCommand.Parameters.AddWithValue("@server", server);
            sqlCommand.Parameters.AddWithValue("@transactionId", transactionId);
            sqlCommand.Parameters.AddWithValue("@price", price);
            sqlCommand.Parameters.AddWithValue("@stocksymbol", stockSymbol);
            sqlCommand.Parameters.AddWithValue("@userid", userId);
            sqlCommand.Parameters.AddWithValue("@quoteservertime", quoteServerTime);
            sqlCommand.Parameters.AddWithValue("@cryptokey", cryptoKey);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();
        }

        public static void LogTransaction(
            string server,
            int transactionId,
            string command,
            int userId,
            decimal funds
        ) {
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, timestamp, server, transactionid, command, userid, funds)
                                       VALUES ('transaction', @curTime, @server, @transactionId, @command, @userid, @funds)";
            sqlCommand.Parameters.AddWithValue("@curTime", DateTime.Now);
            sqlCommand.Parameters.AddWithValue("@server", server);
            sqlCommand.Parameters.AddWithValue("@transactionId", transactionId);
            sqlCommand.Parameters.AddWithValue("@command", command);
            sqlCommand.Parameters.AddWithValue("@userid", userId);
            sqlCommand.Parameters.AddWithValue("@funds", funds);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();
        }

        public static void LogEvent(
            EventType type,
            string server,
            int transactionId,
            string command,
            string message,
            int? userId = null,
            string stockSymbol = null,
            string filename = null
        ) {
            var sqlCommand = SqlHelper.CreateSqlCommand();
            sqlCommand.CommandText = @"INSERT INTO logs (logtype, timestamp, server, transactionid, command, userid, stocksymbol, filename)
                                       VALUES (@type, @curTime, @server, @transactionId, @command, @userid, @stocksymbol, @filename)";
            sqlCommand.Parameters.AddWithValue("@type", type.ToString("F").ToLower());
            sqlCommand.Parameters.AddWithValue("@curTime", DateTime.Now);
            sqlCommand.Parameters.AddWithValue("@server", server);
            sqlCommand.Parameters.AddWithValue("@transactionId", transactionId);
            sqlCommand.Parameters.AddWithValue("@command", command);
            sqlCommand.Parameters.AddWithValue("@userid", userId);
            sqlCommand.Parameters.AddWithValue("@stocksymbol", stockSymbol);
            sqlCommand.Parameters.AddWithValue("@filename", filename);
            sqlCommand.Prepare();
            sqlCommand.ExecuteNonQuery();
        }

        public enum EventType
        {
            System,
            Error,
            Debug
        }
    }
}