using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void Sell(string userid, JObject commandParams)
        {
            /*
                - check quote server for current stock price
                - check stock data table for amount of stocks the user owns
                - calculate if user has enough to sell, stock amount * current price > selling price
                - update transaction server
             */

             //Unpack JObject
            var sellAmount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stockSymbol"].ToString();

            // Get current stock price -- Not implemented
            var stockPrice = (decimal)TransactionHelper.GetStockPrice(stockSymbol);

            // Get amount of stocks user owns. -- Not implemented
            var stockAmount = TransactionHelper.GetStocks(userid, stockSymbol);

            // Check that user has enough stocks to sell.
            if(sellAmount > stockAmount*stockPrice)
            {
                Console.WriteLine("Insufficient stocks ({0}) at selling price ({1}), current stock price: {2}", stockAmount.ToString(), sellAmount.ToString(), stockPrice.ToString());
                throw new System.InvalidOperationException("Insufficient stocks");
                exit;
            }
            else
            {
                // Set POSITIVE balance change
                var balanceChange = stockAmount*stockPrice*1;
                // Set NEGATIVE stockAmount (to remove from stocks table in COMMIT_SELL)
                stockAmount = -stockAmount;
                TransactionHelper.AddTransaction(userid, (decimal)balance, stockSymbol, "SELL", balanceChange, stockAmount, true);
            }
        } 
    }
}
