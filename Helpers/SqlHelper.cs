using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Titanoboa
{
    public static class SqlHelper
    {
        private static string sqlHost = Environment.GetEnvironmentVariable("SQL_HOST") ?? "localhost";
        private static string connectionString = $"Server={sqlHost};Database=scaley_abilities;Uid=scaley;Pwd=abilities;Enlist=false";

        public static async Task<NpgsqlConnection> GetConnection()
        {
            var connection = new NpgsqlConnection(connectionString);

            var connected = false;
            while (!connected)
            {
                try
                {
                    await connection.OpenAsync();
                    connected = true;
                }
                catch (DbException ex)
                {
                    Console.Error.WriteLine($"\n!!!!! Unable to connect to Database, retrying... ({ex.Message})\n");
                    await Task.Delay(1000);
                }
            }

            return connection;
        }

        public static NpgsqlCommand GetCommand(DbConnection connection)
        {
            return new NpgsqlCommand(null, (NpgsqlConnection)connection);
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