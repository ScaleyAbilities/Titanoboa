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

        private static BlockingCollection<String> logQueue;
        private static string filename = @"log.txt";
        
        public int WorkId;
        private static StreamWriter sw;

        static Logger()
        {
            logQueue = new BlockingCollection<String>();
            Task.Run(messageConsumer);
        }

        private static void messageConsumer() 
        {
            using (sw = File.AppendText(filename))
            {
                foreach(var msg in logQueue.GetConsumingEnumerable())
                {
                    sw.WriteLine(msg);
                }
            }
        }

        public void LogCommand(User user = null, String command = null, decimal? amount = null, string stockSymbol = null, string dumpfilename = null)
        {
            string logCommand = $"command, {command}, {WorkId}{Program.InstanceId}, {user.Id}, {amount}, {stockSymbol}, {dumpfilename}";
            logQueue.Add(logCommand); 
        }

        public void LogQuoteServer(User user, decimal? amount, string quoteStockSymbol, string quoteUserId, string quoteServerTime, string cryptoKey)
        {
            string logCommand = $"quote, {WorkId}{Program.InstanceId}, {user.Id}, {amount}, {quoteStockSymbol}, {quoteUserId}, {quoteServerTime}, {cryptoKey}";
            logQueue.Add(logCommand); 
        }

        public void LogEvent(EventType type, string message, User user = null, decimal? amount = null, string stockSymbol = null, string filename = null)
        {
            string logCommand = $"{type}, {message}, {WorkId}{Program.InstanceId}, {user.Id}, {amount}, {stockSymbol}, {filename}";
            logQueue.Add(logCommand); 
        }

        public void LogTransaction(Transaction transaction)
        {
            string message;
            if (transaction.Type == "trigger")
                message = $"{transaction.Command} (trigger, stock {transaction.StockSymbol} at ${transaction.StockPrice})";
            else
                message = $"{transaction.Command} ({transaction.Type}, stock {transaction.StockSymbol})";

            string logCommand = $"transaction, {message}, {WorkId}{Program.InstanceId}, {transaction.User.Id}, {transaction.BalanceChange}";
            logQueue.Add(logCommand); 
        }
    }
}
