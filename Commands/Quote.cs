using System;

namespace Titanoboa
{
    public static partial class Commands
    {
        /*
            Quote command flow:
            1- Hit quote server for most up-to-date price
            2- Log event in Logs table
         */
        public static decimal Quote(string stockSymbol) 
        {
            decimal price = TransactionHelper.GetStockPrice(stockSymbol);
            // TO DO -- fix parameters
            LogHelper.LogQuoteSever("srv", 0, price, stockSymbol, 0, DateTime.Now);
            return price;
        }
    }
}