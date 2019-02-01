namespace Titanoboa
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal BalanceChange { get; set; }
        public string StockSymbol { get; set; }
        public int? StockAmount { get; set; }
        public decimal? StockPrice { get; set; }
    }
}