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
        public static void Dumplog(string userid, JObject commandParams) 
        {
            if(length(commandParams) > 1){
                DumplogUserTans(userid, commandParams);
            }
            else if(length(commandParams) == 1){
                DumplofTrans(userid, commandParams);
            }
        }

        private static void DumplogTans(string userid, JObject commandParams)
        {
            var filename = commandParams["filename"];
            bool isAdmin;
            if(isAdmin = TransactionHelper.IsAdmin(userid))
            {
                Console.WriteLine("Getting all transactions...");
                JObject trans = TransactionHelper.GetTransactions(userid, isAdmin);
            
                // TO DO - write xml to user specified file.
            }
            else
            {
                Console.WriteLine("Inadequite permissions to access all transactions.");
                throw new System.InvalidOperationException("Invalid permissions");
            }
        }
        
        private static void DumplogUserTans(string userid, JObject commandParams)
        {
            var filename = commandParams["filename"];
            JObject trans = TransactionHelper.GetTransactions(userid, false);

            // TO DO - write xml to user specified file.
        }
    }
}
