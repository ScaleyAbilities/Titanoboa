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

            if (!string.IsNullOrEmpty(username))
            {
                var user = TransactionHelper.GetUser(username);
                Program.Logger.LogCommand(user, null, null, filename);
                Program.Logger.CommitLogs();
                Logger.WaitForTasks();
                LogXmlHelper.CreateLog(filename, user);
            }
            else
            {
                Program.Logger.LogCommand(null, null, null, filename);
                Program.Logger.CommitLogs();
                Logger.WaitForTasks();
                LogXmlHelper.CreateLog(filename);
            }
        }
    }
}
