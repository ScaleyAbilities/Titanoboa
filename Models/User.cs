namespace Titanoboa
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public decimal Balance { get; set; }
        public decimal? PendingBalance { get; set; }
    }
}