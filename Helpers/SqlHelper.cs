using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class SqlHelper
    {
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