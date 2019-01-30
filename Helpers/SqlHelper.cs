using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class SqlHelper
    {
        private static string sqlHost = Environment.GetEnvironmentVariable("SQL_HOST") ?? "localhost";
        private static string connectionString = $"Server={sqlHost};Database=scaley_abilities;Uid=scaley;Pwd=abilities;";  
        public static void openSqlConnection() {
            Program.connection = new MySqlConnection(connectionString);
            try
            {
                // Open Connection
                Console.WriteLine("Connecting to Daboia...");
                Program.connection.Open();   
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        public static bool closeSqlConnection() {
            Program.connection.Close();
            Console.WriteLine("Done.");
            return true;
        }
        
         public static MySqlCommand CreateSqlCommand() {
            MySqlCommand command = new MySqlCommand();
            command.Connection = Program.connection;
            return command;
        }
    }
}