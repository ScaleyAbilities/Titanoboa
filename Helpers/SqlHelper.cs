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
        public static bool openSqlConnection() {
            
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
            }
            return true;
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

        public static bool ExcecuteNonQuerySqlCommand(MySqlCommand command) {
            try 
            {
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return false;
            }
            return true;  
        }

        public static object ExcecuteScalarSqlCommand(MySqlCommand command) {
            object res;
            try 
            {
                res = command.ExecuteScalar();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
            return res;  
        }
    }
}