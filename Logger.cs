using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Npgsql;

namespace Titanoboa
{
    public class Logger
    {
        public enum EventType
        {
            System,
            Error,
            Debug
        }
        
        private StringBuilder logString = new StringBuilder(Program.ServerName, 500);
        private bool committed = false;

        public void LogCommand(User user = null, String command = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            logString.AppendLine($"c,{command},{user.Username},{amount},{stockSymbol},{filename},{Timestamp()}");
        }

        public void LogEvent(EventType type, string message, User user = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            // Note, message intentionally goes at the end so it can include commas
            logString.AppendLine($"e,{type},{user.Username},{amount},{stockSymbol},{filename},{Timestamp()},{message}");
        }

        public void LogTransaction(Transaction transaction)
        {
            string message;
            if (transaction.Type == "trigger")
                message = $"{transaction.Command} (trigger, stock {transaction.StockSymbol} at ${transaction.StockPrice})";
            else
                message = $"{transaction.Command} ({transaction.Type}, stock {transaction.StockSymbol})";

            // Note, message intentionally goes at the end so it can include commas
            logString.AppendLine($"t,{transaction.User.Username},{transaction.BalanceChange},{Timestamp()},{message}");
        }

        public void CommitLog()
        {
            if (committed)
                throw new InvalidOperationException("Cannot commit log twice");

            if (!logString.ToString().Contains(Environment.NewLine))
                return; // Nothing to commit
    
            RabbitHelper.PushLogEntry(logString.ToString());
            committed = true;
        }

        private static string Timestamp()
        {
            return Math.Round(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();
        }
    }
}
