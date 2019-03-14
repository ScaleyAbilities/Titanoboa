using System;
using System.Data;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public partial class Commands
    {
        /*
            Set Buy Trigger command flow:
            1- Get current user
            2- Find the stock trigger entry in transaction table
            3- Calculate stock amount (only whole stocks, based on user spending and stock price)
            3- Update spending balance, and number of stocks in transactions table
         */
        public void SetBuyTrigger(string username, JObject commandParams) 
        {
            ParamHelper.ValidateParamsExist(commandParams, "price", "stock");

            // Unpack JObject
            var buyPrice = (decimal)commandParams["price"];
            var stockSymbol = commandParams["stock"].ToString();

            // Get users current balance
            var user = databaseHelper.GetUser(username, true);

            Program.Logger.LogCommand(user, buyPrice, stockSymbol);

            var buyTrigger = databaseHelper.GetTriggerTransaction(user, stockSymbol, "BUY_TRIGGER");
            if (buyTrigger == null)
            {
                throw new InvalidOperationException("No existing trigger");
            }

            databaseHelper.SetTransactionStockPrice(ref buyTrigger, buyPrice);
        } 
    }
}
