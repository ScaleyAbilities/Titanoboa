using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Titanoboa
{
    public static partial class Commands
    {
        /*
            Buy command flow:
            1- Buy current user balance
            2- Get current stock price
            3- Calculate stock amount (only whole stocks)
            3- Insert buy into transactions table, *set pending flag to true*
         */

        public static void Buy(string userid, string stockSymbol, decimal amount) 
        {
            try
            {
                // Get users current balance
                var userbalance = TransactionHelper.GetUserBalance(userid);
                if(userbalance < amount)

                {
                    // Throw insufficient funds exception
                }

                // Get current stock price -- TO DO in helper
                var stockprice = (decimal)TransactionHelper.GetStockPrice(stockSymbol);
                if(amount < stockprice)

                {
                    // Throw not enough money for stock expection
                }

                var stockAmount = (int)Math.Floor(amount/stockprice);

                TransactionHelper.AddTransaction(userid, userbalance, stockSymbol, "BUY", stockAmount*stockprice, stockAmount, true);
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
            }
        } 
    }
}
