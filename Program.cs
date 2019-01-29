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
    
            string command = json["command"].ToString();
            string userid = json["userid"].ToString();
            JObject commandParams = (JObject)json["params"];

            switch (command)
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
                    break;


            }
        }

        static void Main(string[] args)
        {
            SqlHelper.openSqlConnection();
                
            // Run through queue -- TODO
            while(!Queue.isempty())
            {
                string packet = Queue.pop();
                RunCommands(packet);
            }
            
            //Close connection
            SqlHelper.closeSqlConnection();
        }
    }
}
