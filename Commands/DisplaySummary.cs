using System;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace Titanoboa
{
    public partial class CommandHandler
    {
        /*
            DisplaySummary command flow:
            - get transactions by user
            - get user's balance
            - get user's existing triggers
         */
        public void DisplaySummary()
        {
            // TODO: this
            var user = databaseHelper.GetUser(username);
            logger.LogCommand(user);
        }
    }
}
