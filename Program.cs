using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Titanoboa
{
    class Program
    {
        internal static readonly string ServerName = Environment.GetEnvironmentVariable("SERVER_NAME") ?? "Titanoboa";
        internal static Logger Logger = null;
        internal static string CurrentCommand = null;

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

            CurrentCommand = json["cmd"].ToString().ToUpper();
            string username = json["usr"].ToString();
            JObject commandParams = (JObject)json["params"];

            try
            {
                // Set up a logger for this unit of work
                Logger = new Logger();
            }
            catch (DbException ex)
            {
                Console.Error.WriteLine($"Unable to create logger due to SQL error: {ex.Message}");
                return;
            }

            using (var connection = SqlHelper.GetConnection())
            using (var dbHelper = new DatabaseHelper(connection))
            {
                string error = null;
                var commands = new Commands(dbHelper);
                try 
                {
                    switch (CurrentCommand)
                    {
                        case "QUOTE":
                            commands.Quote(username, commandParams);
                            break;
                        case "ADD":
                            commands.Add(username, commandParams);
                            break;
                        case "BUY":
                            commands.Buy(username, commandParams);
                            break;
                        case "COMMIT_BUY":
                            commands.CommitBuy(username);
                            break;
                        case "CANCEL_BUY":
                            commands.CancelBuy(username);
                            break;
                        case "SELL":
                            commands.Sell(username, commandParams);
                            break;
                        case "COMMIT_SELL":
                            commands.CommitSell(username);
                            break;
                        case "CANCEL_SELL":
                            commands.CancelSell(username);
                            break;
                        case "SET_BUY_AMOUNT":
                            commands.SetBuyAmount(username, commandParams);
                            break;
                        case "SET_BUY_TRIGGER":
                            commands.SetBuyTrigger(username, commandParams);
                            break;
                        case "CANCEL_SET_BUY":
                            commands.CancelSetBuy(username, commandParams);
                            break;
                        case "SET_SELL_AMOUNT":
                            commands.SetSellAmount(username, commandParams);
                            break;
                        case "SET_SELL_TRIGGER":
                            commands.SetSellTrigger(username, commandParams);
                            break;
                        case "CANCEL_SET_SELL":
                            commands.CancelSetSell(username, commandParams);
                            break;
                        case "DUMPLOG":
                            commands.Dumplog(username, commandParams);
                            break;
                        case "DISPLAY_SUMMARY":
                            commands.DisplaySummary(username);
                            break;
                        default:
                            Console.Error.WriteLine($"Unknown command '{CurrentCommand}'");
                            break;
                    }

                    Logger.CommitLogs();
                    dbHelper.CommitAllChanges();
                }
                catch (ArgumentException ex)
                {
                    error = $"Invalid parameters for command '{CurrentCommand}': {ex.Message}";
                }
                catch (InvalidOperationException ex)
                {
                    error = $"Command '{CurrentCommand}' could not be run: {ex.Message}";
                }
                catch (DbException ex)
                {
                    error = $"!!!SQL ERROR!!! {ex.Message}";
                }
                catch (Exception ex)
                {
                    error = $"!!!UNEXPECTED ERROR!!! {ex.Message}";
                }

                if (error != null)
                {
                    Console.Error.WriteLine(error);
                    dbHelper.RollbackAllChanges();
                    Logger = new Logger();
                    Logger.LogEvent(Logger.EventType.Error, error);
                    Logger.CommitLogs();
                }
            }

            // Clear the logger now that we are done this unit of work
            Logger = null;
            CurrentCommand = null;
        }

        static async Task Main(string[] args)
        {
            RabbitHelper.CreateConsumer(RunCommands);
            // TODO: Need to make rabbit queue for sending triggers to Twig
            
            Console.WriteLine("Titanoboa running...");

            if (args.Contains("--no-input"))
            {
                while (true)
                {
                    await Task.Delay(int.MaxValue);
                }
            } 
            else 
            {
                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
