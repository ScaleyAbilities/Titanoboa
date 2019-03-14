using System;
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

        public CommandHandler(string username, string command, JObject commandParams, DatabaseHelper databaseHelper, Logger logger)
        {
            this.username = username;
            this.command = command;
            this.commandParams = commandParams;
            this.databaseHelper = databaseHelper;
            this.logger = logger;
        }

        public void Run()
        {
            switch (command)
            {
                case "QUOTE":
                    Quote();
                    break;
                case "ADD":
                    Add();
                    break;
                case "BUY":
                    Buy();
                    break;
                case "COMMIT_BUY":
                    CommitBuy();
                    break;
                case "CANCEL_BUY":
                    CancelBuy();
                    break;
                case "SELL":
                    Sell();
                    break;
                case "COMMIT_SELL":
                    CommitSell();
                    break;
                case "CANCEL_SELL":
                    CancelSell();
                    break;
                case "SET_BUY_AMOUNT":
                    SetBuyAmount();
                    break;
                case "SET_BUY_TRIGGER":
                    SetBuyTrigger();
                    break;
                case "CANCEL_SET_BUY":
                    CancelSetBuy();
                    break;
                case "SET_SELL_AMOUNT":
                    SetSellAmount();
                    break;
                case "SET_SELL_TRIGGER":
                    SetSellTrigger();
                    break;
                case "CANCEL_SET_SELL":
                    CancelSetSell();
                    break;
                case "DUMPLOG":
                    Dumplog();
                    break;
                case "DISPLAY_SUMMARY":
                    DisplaySummary();
                    break;
                default:
                    Console.Error.WriteLine($"Unknown command '{command}'");
                    break;
            }
        }

        private void CheckParams(params string[] expectedParams)
        {
            ParamHelper.ValidateParamsExist(commandParams, expectedParams);
        }

    }
}