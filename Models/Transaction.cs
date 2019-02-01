namespace Titanoboa
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal BalanceChange { get; set; }
        public string StockSymbol { get; set; }
        public int? StockAmount { get; set; }
        public decimal? StockPrice { get; set; }

        // Used when logging to make sure we don't double-log a transaction. Dumb but it works.
        public bool HasBeenLogged = false;
    }
}