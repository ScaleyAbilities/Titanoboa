using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    class Program
    {
        internal static MySqlConnection Connection;
        internal static readonly string ServerName = "Titanoboa";
        internal static Logger Logger = null;

        static void RunCommands(JObject json)
        {
            try
            {
                ParamHelper.ValidateParamsExist(json, "cmd", "usr");
            }                
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Error in Queue JSON: {ex.Message}");
                return;
            }

            string command = json["cmd"].ToString().ToUpper();
            string username = json["usr"].ToString();
            JObject commandParams = (JObject)json["params"];

            try
            {
                // Set up a logger for this unit of work
                Logger = new Logger();
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Unable to create logger due to SQL error: {ex.Message}");
                return;
            }

            try 
            {
                switch (command)
                {
                    case "ADD":
                        Commands.Add(username, commandParams);
                        break;
                    case "BUY":
                        Commands.Buy(username, commandParams);
                        break;
                    case "QUOTE":
                        Commands.Quote(username, commandParams);
                        break;
                    case "COMMIT_BUY":
                        Commands.CommitBuy(username);
                        break;
                    case "CANCEL_BUY":
                        Commands.CancelBuy(username);
                        break;
                    case "SELL":
                        Commands.Sell(username, commandParams);
                        break;
                    case "COMMIT_SELL":
                        Commands.CommitSell(username);
                        break;
                    case "SET_BUY_AMOUNT":
                        Commands.SetBuyAmount(username, commandParams);
                        break;
                    case "SET_BUY_TRIGGER":
                        Commands.SetBuyTrigger(username, commandParams);
                        break;
                    case "CANCEL_SET_BUY":
                        Commands.CancelSetBuy(username, commandParams);
                        break;
                    case "SET_SELL_AMOUNT":
                        Commands.SetSellAmount(username, commandParams);
                        break;
                    case "SET_SELL_TRIGGER":
                        Commands.SetSellTrigger(username, commandParams);
                        break;
                    case "CANCEL_SET_SELL":
                        Commands.CancelSetSell(username, commandParams);
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
                Logger.LogEvent(Logger.EventType.Error, ex.Message, null, null, null, command);
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Command '{command}' could not be run: {ex.Message}");
                Logger.LogEvent(Logger.EventType.Error, ex.Message, null, null, null, command);
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Command '{command}' encountered a SQL error: {ex.Message}");
                Logger.LogEvent(Logger.EventType.Error, $"SQL ERROR!!! {ex.Message}", null, null, null, command);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Command '{command}' encountered an unexpected error: {ex.Message}");
                Logger.LogEvent(Logger.EventType.Error, $"UNEXPECTED ERROR!!! {ex.Message}", null, null, null, command);
            }

            // Clear the logger now that we are done this unit of work
            Logger = null;
        }

        static void Main(string[] args)
        {
            SqlHelper.OpenSqlConnection();
            RabbitHelper.CreateConsumer(RunCommands);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
            
            //Close connection
            SqlHelper.CloseSqlConnection();
        }
    }
}
