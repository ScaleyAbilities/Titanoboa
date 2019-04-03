using System;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
        public async Task DisplaySummary()
        {
            // TODO: this
            var user = await databaseHelper.GetUser(username);
            logger.LogCommand(user, command);
        }
    }
}
