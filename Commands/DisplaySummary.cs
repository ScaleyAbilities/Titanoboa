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
            DisplaySummary command flow:
            - get transactions by user
            - get user's balance
            - get user's existing triggers
         */
        public static void DisplaySummary(string username)
        {
            var user = TransactionHelper.GetUser(username);
            decimal? balance = user.PendingBalance;
            JObject trans = TransactionHelper.GetUserLogs(user);
        }
    }
}
