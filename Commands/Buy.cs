using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        /*
            Buy command flow:
            1- Get current user balance
            2- Get current stock price
            3- Calculate stock amount (only whole stocks)
            3- Insert buy into transactions table, *set pending flag to true*
         */
        public static void Buy(string userid, JObject commandParams) 
        {
            //Unpack JObject
            var amount = (decimal)commandParams["amount"];
            var stockSymbol = commandParams["stockSymbol"].ToString();

            // Get users current balance
            var balance = TransactionHelper.GetUserBalance(userid);
            
            //Check if user exists
            if(balance == null)
            {
                throw new System.InvalidOperationException("User does not exist.");
            } 
            else if((decimal)balance < amount)
            {
                throw new System.InvalidOperationException("Insufficient funds.");
            }

            // Get current stock price -- TO DO in helper
            var stockPrice = (decimal)TransactionHelper.GetStockPrice(stockSymbol);
            if(amount < stockPrice)
            {
                throw new System.InvalidOperationException("Not enough money for stock purchase.");
            }

            var stockAmount = (int)Math.Floor(amount/stockPrice);
            var balanceChange = stockAmount*stockPrice*-1;

            TransactionHelper.AddTransaction(userid, (decimal)balance, stockSymbol, "BUY", balanceChange, stockAmount, true);
        } 
    }
}
