using System;
using System.Collections.Generic;

namespace Titanoboa.Models
{
    public class TriggerList : Dictionary<String, Dictionary<String, SortedDictionary<decimal, User>>>
    {
        private Dictionary<String, Dictionary<String ,SortedDictionary<decimal, User>>> StockTriggers;
        private SortedDictionary<decimal, User> PriceValues;
        private String Buy { get; set; }
        private String Sell { get; set; }
        public TriggerList()
        {
            // Create new stock symbol entry in dictionary
            // Maps from Stock to Buy/Sell to Price
            // StockTriggers = new Dictionary<String, Dictionary<String, SortedDictionary<decimal, User>>>();
        }

        public void Add(String Symbol, String Choice, decimal Price, User u)
        {
            Choice = Choice.ToLower();
            // Create Trigger object with user and price
            if(this.StockTriggers.ContainsKey(Symbol))
            {
                StockTriggers[Symbol][Choice][Price] = u;
            }
            else 
            {
                this.StockTriggers.Add(Symbol, new Dictionary<String, SortedDictionary<decimal, User>>());
                this[Symbol].Add("buy", new SortedDictionary<decimal, User>());
                this[Symbol].Add("sell", new SortedDictionary<decimal, User>());
                StockTriggers[Symbol][Choice][Price] = u;
            }
        }

        public void checkStockTriggers(String Symbol, decimal StockPrice)
        {
            var Stock = this[Symbol];

            foreach(var request in Stock) { // 
                foreach(var req in request.Value ) {
                    if(request.Key.Equals("buy") && req.Key >= StockPrice) {
                        // Buy
                    }
                    else if(request.Key.Equals("sell") && req.Key <= StockPrice) {
                        // Sell
                    }
                }
            }
        }
    }
}