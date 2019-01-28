using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    static class Helper
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

        public static bool AddTransaction(string userid, decimal balance, string stocksymbol, string command, decimal balancechange, int stockamount, bool pending){
            MySqlCommand command = CreateSqlCommand();
            command.Prepare();
            command.CommandText = "INSERT INTO transactions (userid, balance, stocksymbol, command, balancechange, stockamount, pending) \
                                    values ('@userid', '@balance', '@stocksymbol', '@command', '@balancechange', '@stockamount', '@pending')";
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@stocksymbol", stocksymbol);
            command.Parameters.AddWithValue("@command", command);
            command.Parameters.AddWithValue("@balancechange", balancechange);
            command.Parameters.AddWithValue("@stockamount", stockamount);
            command.Parameters.AddWithValue("@pending", pending);
            return ExcecuteNonQuerySqlCommand(command)
        }

        public static bool AddUser(string userid, decimal balance) {
            MySqlCommand command = CreateSqlCommand();
            command.CommandText = "INSERT INTO users (balance, userid) Values ('@balance', '@userid')";
            command.Prepare();
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@userid", userid);
            return ExcecuteNonQuerySqlCommand(command);
        }

        public static decimal GetUserBalance(string userid) {
            MySqlCommand command = CreateSqlCommand();
            command.CommandText = "SELECT balance FROM users WHERE userid==@userid";
            command.Prepare();
            command.Parameters.AddWithValue("@userid", userid);
            var balance = (decimal)ExcecuteScalarSqlCommand(command);
            return balance;
        }

        public static bool UpdateUserBalance(string userid, decimal balance) {
            MySqlCommand command = CreateSqlCommand();
            command.CommandText = "UPDATE users SET balance = @balance WHERE userid==@userid";
            command.Prepare();
            command.Parameters.AddWithValue("@balance", balance);
            command.Parameters.AddWithValue("@userid", userid);
            return ExcecuteNonQuerySqlCommand(command);
        }
    }
}