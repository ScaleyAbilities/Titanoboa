using System;
using System.Data;
using System.Threading;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class SqlHelper
    {
        private static MySqlConnection connection;
        private static string sqlHost = Environment.GetEnvironmentVariable("SQL_HOST") ?? "localhost";
        private static string connectionString = $"Server={sqlHost};Database=scaley_abilities;Uid=scaley;Pwd=abilities;";

        public static void OpenSqlConnection()
        {
            connection = new MySqlConnection(connectionString);

            var connected = false;
            while (!connected)
            {
                try
                {
                    connection.Open();
                    connected = true;
                }
                catch (MySqlException ex)
                {
                    Console.Error.WriteLine($"Unable to connect to Database, retrying... ({ex.Message})");
                    Thread.Sleep(3000);
                }
            }
        }

        public static bool CloseSqlConnection()
        {
            connection.Close();
            Console.WriteLine("Done.");
            return true;
        }

        public static MySqlCommand CreateSqlCommand()
        {
            MySqlCommand command = new MySqlCommand();
            command.Connection = connection;
            return command;
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