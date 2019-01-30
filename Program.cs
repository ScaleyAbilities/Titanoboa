using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    class Program
    {
        internal static MySqlConnection connection;

        static void RunCommands(JObject json)
        {
            try
            {
                ParamHelper.ValidateParamsExist(json, "cmd", "uid", "params");
            }                
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Error in Queue JSON: {ex.Message}");
                return;
            }

            string command = json["cmd"].ToString();
            string userid = json["uid"].ToString();
            JObject commandParams = (JObject)json["params"];

            try 
            {
                switch (command.ToUpperInvariant())
                {
                    case "ADD":
                        Commands.Add(userid, commandParams);
                        break;
                    case "BUY":
                        Commands.Buy(userid, commandParams);
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
                    default:
                        Console.Error.WriteLine($"Unknown command '{command}'");
                        break;
                }
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Invalid parameters for command '{command}': {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Command '{command}' could not be run: {ex.Message}");
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Command '{command}' encountered a SQL error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Command '{command}' encountered an unexpected error: {ex.Message}");
            }
        }

        static void Main(string[] args)
        {
            SqlHelper.openSqlConnection();
            RabbitHelper.CreateConsumer(RunCommands);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
            
            //Close connection
            SqlHelper.closeSqlConnection();
        }
    }
}
