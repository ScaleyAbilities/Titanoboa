using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Titanoboa
{
    class Program
    {
        static void RunCommands(string packet)
        {
            //TODO
            string command = packet.body.command;
            string userid = packet.body.userid;

            switch (command)
            {
                case "ADD":
                    Commands.Add(userid, json.body.amount, connection);
                    break;
                case "QUOTE":
                    break;
                case "COMMIT_BUY":
                    break;
                case "CANCEL_BUY":
                    break;
                case "SELL":
                    break;
                case "COMMIT_SELL":
                    break;
                case "SET_BUY_AMOUNT":
                    break;
                case "CANCEL_SET_BUY":
                    break;
                case "SET_BUY_TRIGGER":
                    break;
                case "SET_SELL_AMOUNT":
                    break;
                case "SET_SELL_TRIGGER":
                    break;
                case "CANCEL_SET_SELL":
                    break;
                case "DUMPLOG_USER":
                    break;
                case "DUMPLOG_ALL":
                    break;
                case "DISPLAY_SUMMARY":
                    break;

            }
        }

        static void Main(string[] args)
        {
            //DB config -- TODO
            string connectionString = "";

            //Establish DB Connection
            MySqlConnection connection = new MySqlConnection(connectionString);

            try
            {
                Console.WriteLine("Connecting to MySQL...");
                connection.Open();
                
                //Run through queue -- TODO
                while(!Queue.isempty())
                {
                    string packet = Queue.pop();
                    RunCommands(packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            connection.Close();
            Console.WriteLine("Done.");

        }
    }
}
