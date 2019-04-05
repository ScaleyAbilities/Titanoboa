using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        private string username;
        private string command;
        private JObject commandParams;
        private DatabaseHelper databaseHelper;
        private Logger logger;
        private long taskId;
        private string returnRef;
        
        public JObject returnValue { get; set; }

        public CommandHandler(string username, string command, JObject commandParams, DatabaseHelper databaseHelper, Logger logger, long taskId, string returnRef = null)
        {
            this.username = username;
            this.command = command;
            this.commandParams = commandParams;
            this.databaseHelper = databaseHelper;
            this.logger = logger;
            this.taskId = taskId;
            this.returnRef = returnRef;
        }

        public async Task Run()
        {
            Task commandTask = null;
            switch (command)
            {
                case "QUOTE":
                    commandTask = Quote();
                    break;
                case "ADD":
                    commandTask = Add();
                    break;
                case "BUY":
                    commandTask = Buy();
                    break;
                case "COMMIT_BUY":
                    commandTask = CommitBuy();
                    break;
                case "CANCEL_BUY":
                    commandTask = CancelBuy();
                    break;
                case "SELL":
                    commandTask = Sell();
                    break;
                case "COMMIT_SELL":
                    commandTask = CommitSell();
                    break;
                case "CANCEL_SELL":
                    commandTask = CancelSell();
                    break;
                case "SET_BUY_AMOUNT":
                    commandTask = SetBuyAmount();
                    break;
                case "SET_BUY_TRIGGER":
                    commandTask = SetBuyTrigger();
                    break;
                case "COMMIT_BUY_TRIGGER":
                    commandTask = CommitBuyTrigger();
                    break;
                case "CANCEL_SET_BUY":
                    commandTask = CancelSetBuy();
                    break;
                case "SET_SELL_AMOUNT":
                    commandTask = SetSellAmount();
                    break;
                case "SET_SELL_TRIGGER":
                    commandTask = SetSellTrigger();
                    break;
                case "COMMIT_SELL_TRIGGER":
                    commandTask = CommitSellTrigger();
                    break;
                case "CANCEL_SET_SELL":
                    commandTask = CancelSetSell();
                    break;
                case "DUMPLOG":
                    commandTask = Dumplog();
                    break;
                case "DISPLAY_SUMMARY":
                    commandTask = DisplaySummary();
                    break;
                default:
                    Console.Error.WriteLine($"Unknown command '{command}'");
                    break;
            }

            if (commandTask != null)
                await commandTask;
        }

        private void CheckParams(params string[] expectedParams)
        {
            ParamHelper.ValidateParamsExist(commandParams, expectedParams);
        }

    }
}