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

        public static void Buy(string userid, string stocksymbol, decimal amount, MySqlConnection connection) 
        {
            try
            {
                //Update DB 
                MySqlCommand command = new MySqlCommand();
                command.Connection = connection;

                // Get users current balance
                var userbalance = Helper.GetUserBalance(userid);
                if(userbalance < amount)
                {
                    // Throw insufficient funds exception
                }

                // Get current stock price -- TO DO in helper
                var stockprice = (decimal)Helper.GetStockPrice(StockSymbol);
                if(amount < stockprice)
                {
                    // Throw not enough money for stock expection
                }

                var stockamount = ((integer)amount/stockprice)

                Helper.AddTransaction(userid, userbalance, stocksymbol, "BUY", stockamount*stockprice, stockamount, true)
            }
        } 
    }
}