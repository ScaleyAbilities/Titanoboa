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
            Logger logger;

            try
            {
                // Set up a logger for this unit of work
                logger = new Logger(command);
            }
            catch (DbException ex)
            {
                Console.Error.WriteLine($"Unable to create logger due to SQL error: {ex.Message}");
                return;
            }

            using (var connection = SqlHelper.GetConnection())
            using (var dbHelper = new DatabaseHelper(connection, logger))
            {
                string error = null;
                var commandHandler = new CommandHandler(username, command, commandParams, dbHelper, logger);
                try 
                {
                    commandHandler.Run();
                    logger.CommitLogs();
                    dbHelper.CommitAllChanges();
                }
                catch (ArgumentException ex)
                {
                    error = $"Invalid parameters for command '{command}': {ex.Message}";
                }
                catch (InvalidOperationException ex)
                {
                    error = $"Command '{command}' could not be run: {ex.Message}";
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
                    logger = new Logger(command);
                    logger.LogEvent(Logger.EventType.Error, error);
                    logger.CommitLogs();
                }
            }
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
