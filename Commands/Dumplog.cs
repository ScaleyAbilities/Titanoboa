using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace Titanoboa
{
    public static partial class Commands
    {
        /*
            Dumplog command flow:
            - determin if for single user trans or all trans
            if all:
                - check user is admin
                - get transactions
                - write xml
            if single user:
                - get transactions by user
                - write xml
         */
        public static void Dumplog(string username, JObject commandParams)
        {
            ParamHelper.ValidateParamsExist(commandParams, "filename");

            var filename = (string)commandParams["filename"];

            if (username != null)
            {
                DumplogUser(username, filename);
            }
            else
            {
                DumplogAll(filename);
            }
        }

        private static void DumplogAll(string filename)
        {
            // For now we just assume the user is allowed to do this
            Console.WriteLine("Getting all transactions...");
            JObject trans = TransactionHelper.GetAllLogs();
            
            // TO DO - write xml to user specified file.
        }
        
        private static void DumplogUser(string username, string filename)
        {
            var user = TransactionHelper.GetUser(username);
            JObject trans = TransactionHelper.GetUserLogs(user);
        }
    }
}
