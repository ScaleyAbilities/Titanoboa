namespace Titanoboa
{
    public class Transaction
    {
        public ulong Id { get; set; }
        public User User { get; set; }
        public string Command { get; set; }
        public decimal BalanceChange { get; set; }
        public string StockSymbol { get; set; }
        public int? StockAmount { get; set; }
        public decimal? StockPrice { get; set; }
        public string Type { get; set; }
    }
}