using System;

namespace Titanoboa.Models
{
    public enum TriggerType 
    {
        Buy,
        Sell,
    }
    public class Trigger
    {
        public User User { get; set; }
        public String StockSymbol { get; set; }
        internal TriggerType TriggerType { get; set; }
        public decimal? Price { get; set; }  
    }
}