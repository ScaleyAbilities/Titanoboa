using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Titanoboa
{
    public static class SqlHelper
    {
        private static NpgsqlConnection connection;
        private static string sqlHost = Environment.GetEnvironmentVariable("SQL_HOST") ?? "localhost";
        private static string connectionString = $"Server={sqlHost};Database=scaley_abilities;Uid=scaley;Pwd=abilities;";

        public static NpgsqlConnection GetConnection()
        {
            connection = new NpgsqlConnection(connectionString);

            var connected = false;
            while (!connected)
            {
                try
                {
                    connection.Open();
                    connected = true;
                }
                catch (DbException ex)
                {
                    Console.Error.WriteLine($"\n!!!!! Unable to connect to Database, retrying... ({ex.Message})\n");
                    Thread.Sleep(3000);
                }
            }

            return connection;
        }

        public static NpgsqlCommand GetCommand(DbConnection connection, DbTransaction transaction = null)
        {
            return new NpgsqlCommand(null, (NpgsqlConnection)connection, (NpgsqlTransaction)transaction);
        }

        public static NpgsqlCommand CreateSqlCommand()
        {
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = connection;
            return command;
        }

        public static NpgsqlTransaction StartTransaction()
        {
            return connection.BeginTransaction();
        }

        public static int? ConvertToNullableInt32(object num)
        {
            return Convert.IsDBNull(num) || num == null ? null : (int?)Convert.ToInt32(num);
        }

        public static decimal? ConvertToNullableDecimal(object num)
        {
            return Convert.IsDBNull(num) || num == null ? null : (decimal?)(num);
        }
    }
}