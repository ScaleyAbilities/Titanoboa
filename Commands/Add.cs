using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void Add(string userid, decimal amount, MySqlConnection connection) 
        {
            //Update DB 
            MySqlCommand command = new MySqlCommand();
            command.Connection = connection;

            command.CommandText = "SELECT Balance FROM Users WHERE UserId==@userid";
            command.Prepare();
            command.Parameters.AddWithValue("@userid", userid);
            var balance = (decimal)command.ExecuteScalar();
            balance += amount;

            command.CommandText = "INSERT INTO Users VALUE(@userid, @amount)";
            command.Prepare();

            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@amount", balance);
            command.ExecuteNonQuery();

        } 

    }
}