using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Titanoboa
{
    class Program
    {
        internal static readonly string ServerName = Environment.GetEnvironmentVariable("SERVER_NAME") ?? "Titanoboa";

        internal static ConcurrentQueue<(long id, Task task)> runningTasks = new ConcurrentQueue<(long id, Task task)>();
        internal static ConcurrentDictionary<string, SemaphoreSlim> userLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        internal static string InstanceId = Environment.GetEnvironmentVariable("AUTO_INSTANCE") == "TRUE" ? null : "1";

        private static long nextTaskId = 0;

        static async Task RunCommands(long taskId, JObject json)
        {
            try
            {
                ParamHelper.ValidateParamsExist(json, "cmd");
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Error in Queue JSON: {ex.Message}");
                return;
            }

            var command = json["cmd"].ToString().ToUpper();
            var username = json["usr"]?.ToString();

            // This is used to return values to frontend. It will be null for workload files.
            var returnRef = json["ref"]?.ToString();

            if (command != "DUMPLOG" && string.IsNullOrEmpty(username))
            {
                Console.Error.WriteLine($"Error in Queue JSON: Missing username");
                return;
            }

            JObject commandParams = (JObject)json["params"];
            Logger logger = new Logger(command);
            
            string error = null;
            var errorLevel = 0; // 0 = lowest, 2 = highest
            CommandHandler commandHandler;

            using (var connection = await SqlHelper.GetConnection())
            using (var dbHelper = new DatabaseHelper(connection, logger))
            {
                commandHandler = new CommandHandler(username, command, commandParams, dbHelper, logger, taskId, returnRef);
                try
                {
                    await commandHandler.Run();
                }
                catch (ArgumentException ex)
                {
                    error = $"Invalid parameters for command '{command}': {ex.Message}";
                    errorLevel = 1;
                }
                catch (InvalidOperationException ex)
                {
                    error = $"Command '{command}' could not be run: {ex.Message}";
                    errorLevel = 0;
                }
                catch (DbException ex)
                {
                    error = $"!!!SQL ERROR!!! {ex.Message}";
                    errorLevel = 2;
                }
                catch (Exception ex)
                {
                    error = $"!!!UNEXPECTED ERROR!!! {ex.Message}";
                    errorLevel = 2;
                }

                if (error != null)
                {
                    if (errorLevel > 0) // Only log unexpected errors
                    {
                        Console.Error.WriteLine(error);
                    }
                    logger.LogEvent(Logger.EventType.Error, error);
                }

                logger.CommitLog();
            }

            if (!string.IsNullOrEmpty(returnRef))
            {
                var returnJson = new JObject();
                returnJson.Add("ref", returnRef);
                returnJson.Add("status", error == null ? "ok" : "error");
                returnJson.Add("data", error == null ? (dynamic)commandHandler.returnValue : error);

                RabbitHelper.PushResponse(returnJson);
            }
        }

        static async Task Main(string[] args)
        {
            var quitSignalled = new TaskCompletionSource<bool>();
            Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, eventArgs) => {
                quitSignalled.SetResult(true);
                eventArgs.Cancel = true; // Prevent program from quitting right away
            });

            if (InstanceId == null)
            {
                Console.WriteLine("Getting instance...");
                InstanceId = RabbitHelper.GetInstance();
                Console.WriteLine($"Got instance {InstanceId}");
            }

            RabbitHelper.CreateQueues();
            
            RabbitHelper.CreateConsumer(RabbitConsumer, RabbitHelper.rabbitCommandQueue);
            RabbitHelper.CreateConsumer(RabbitConsumer, RabbitHelper.rabbitTriggerCompleted, 1);
            
            Console.WriteLine("Titanoboa running...");
            Console.WriteLine("Press Ctrl-C to exit.");

            while (true)
            {
                var completed = await Task.WhenAny(quitSignalled.Task, Task.Delay(10000));

                if (completed == quitSignalled.Task)
                    break;

                // We clean up finished tasks every 10 seconds
                CleanupFinishedTasks();
            }

            Console.WriteLine("Quitting...");

            Console.WriteLine("Ending Rabbit connection...");
            RabbitHelper.CloseRabbit();

            Console.WriteLine("Waiting for running tasks to complete...");

            while (!runningTasks.IsEmpty)
            {
                (long id, Task task) taskEntry = (0, null);
                runningTasks.TryDequeue(out taskEntry);
                if (taskEntry.task != null)
                    await taskEntry.task;
            }

            Console.WriteLine("Done.");
            Environment.Exit(0);
        }

        public static async Task WaitForTasksUpTo(long id)
        {
            (long id, Task task) taskEntry = (0, null);
            while (runningTasks.TryPeek(out taskEntry) && taskEntry.id != id)
            {
                runningTasks.TryDequeue(out taskEntry);
                await taskEntry.task;
            }
        }

        private static void CleanupFinishedTasks()
        {
            (long id, Task task) taskEntry = (0, null);
            while (runningTasks.TryPeek(out taskEntry) && taskEntry.task.IsCompleted)
            {
                runningTasks.TryDequeue(out taskEntry);
            }
        }

        private static Task RabbitConsumer(JObject json)
        {
            var id = nextTaskId++;
            var task = RunCommands(id, json);
            runningTasks.Append((id, task));
            return task;
        }
    }
}
