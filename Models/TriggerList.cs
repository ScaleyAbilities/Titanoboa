using System;
using System.Collections.Generic;

namespace Titanoboa.Models
{
    public class TriggerList : Dictionary<String, Dictionary<String, SortedSet<Tuple<decimal, User>>>>
    {
        public void Add(String Symbol, String Choice, decimal Price, User u)
        {
            Choice = Choice.ToLower();
            if(!Choice.Equals("buy") && !Choice.Equals("sell")) return;

            // Checks if the StockSymbol and Buy/Sell keys are in the object, adds if not
            if(!this.ContainsKey(Symbol)) {
                this[Symbol] = new Dictionary<string, SortedSet<Tuple<decimal, User>>>();
                this[Symbol]["buy"] = new SortedSet<Tuple<decimal, User>>();
                this[Symbol]["sell"] = new SortedSet<Tuple<decimal, User>>();
            }

            // Adds the price and user data
            this[Symbol][Choice].Add(Tuple.Create(Price, u));
        }

        public void CheckStockTriggers(String Symbol, decimal StockPrice)
        {
            var BuyStock = this[Symbol]["buy"];
            var SellStock = this[Symbol]["sell"];

            if(BuyStock.Count > 0) {
                while(BuyStock.Max.Item1 >= StockPrice) {
                    BuyStock.Remove(BuyStock.Max);  
                    // TODO: Proper buy
                }
            }
            if(SellStock.Count > 0) {
                while(SellStock.Min.Item1 <= StockPrice) {
                    SellStock.Remove(SellStock.Max);
                    // TODO: Proper sell
                }
            }
        }

        static void Main(string[] args)
        {
            // Display the number of command line arguments:
            System.Console.WriteLine(args.Length + " args");
            User me = new User();
            me.Id = 123;
            me.Username = "Rick";
            TriggerList obj = new TriggerList();
            obj.Add("aks","buy", 3.90m, me);
            obj.Add("aks","buy", 2.99m, me);
            obj.Add("aks","buy", 5m, me);
            obj.Add("aks","buy", 1m, me);
            obj.Add("kek","sell", 4m, me);

            obj.CheckStockTriggers("aks",3m);
            obj.CheckStockTriggers("kek",3m);
            System.Console.WriteLine("Done.");
        }
    }
}