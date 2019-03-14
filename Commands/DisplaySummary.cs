using System;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace Titanoboa
{
    public partial class Commands
    {
        /*
            DisplaySummary command flow:
            - get transactions by user
            - get user's balance
            - get user's existing triggers
         */
        public void DisplaySummary(string username)
        {
            // TODO: this
            var user = databaseHelper.GetUser(username);
            Program.Logger.LogCommand(user);
        }
    }
}
