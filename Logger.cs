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
        
        private StringBuilder logString = new StringBuilder();
        private bool committed = false;

        public void LogCommand(User user = null, String command = null, decimal? amount = null, string stockSymbol = null, string dumpfilename = null)
        {
            logString.AppendLine($"c,{command},{WorkId}{Program.InstanceId},{user.Username},{amount},{stockSymbol},{dumpfilename}");
        }

        public void LogEvent(EventType type, string message, User user = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            logString.AppendLine($"e,{type},\"{message}\",{WorkId}{Program.InstanceId},{user.Username},{amount},{stockSymbol},{filename}");
        }

        public void LogTransaction(Transaction transaction)
        {
            string message;
            if (transaction.Type == "trigger")
                message = $"{transaction.Command} (trigger, stock {transaction.StockSymbol} at ${transaction.StockPrice})";
            else
                message = $"{transaction.Command} ({transaction.Type}, stock {transaction.StockSymbol})";

            logString.AppendLine($"t,\"{message}\",{WorkId}{Program.InstanceId},{transaction.User.Username},{transaction.BalanceChange}");
        }

        public void CommitLog()
        {
            if (committed)
                throw new InvalidOperationException("Cannot commit log twice");

            if (logString.Length < 1)
                return; // Nothing to commit
    
            RabbitHelper.PushLogEntry(logString.ToString());
            committed = true;
        }
    }
}
