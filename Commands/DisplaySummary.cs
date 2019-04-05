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
            var user = await databaseHelper.GetUser(username, true);
            logger.LogCommand(user, command);

            // If we aren't returing values to user, just return here
            if (string.IsNullOrEmpty(returnRef))
                return;

            var summary = await databaseHelper.GetUserSummary(user);

            var ret = new JObject();
            ret.Add("balance", user.Balance);
            ret.Add("pending", user.PendingBalance);
            var stocks = new JObject();
            foreach (var stockAmount in summary)
                stocks.Add(stockAmount.Key, stockAmount.Value);
            
            ret.Add("stocks", stocks);

            returnValue = ret;
        }
    }
}
