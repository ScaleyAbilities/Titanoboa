using System;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace Titanoboa
{
    public partial class CommandHandler
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
        public void Dumplog()
        {
            CheckParams("filename");

            var filename = (string)commandParams["filename"];

            if (!string.IsNullOrEmpty(username))
            {
                var user = databaseHelper.GetUser(username);
                logger.LogCommand(user, null, null, filename);
                logger.CommitLogs();
                Logger.WaitForTasks();
                LogXmlHelper.CreateLog(filename, user);
            }
            else
            {
                logger.LogCommand(null, null, null, filename);
                logger.CommitLogs();
                Logger.WaitForTasks();
                LogXmlHelper.CreateLog(filename);
            }
        }
    }
}
