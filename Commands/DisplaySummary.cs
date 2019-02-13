using System;
using System.Data;
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
            // TODO: this
            var user = TransactionHelper.GetUser(username);
            Program.Logger.LogCommand(user);
        }
    }
}
