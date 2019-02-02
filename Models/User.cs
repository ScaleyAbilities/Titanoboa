namespace Titanoboa
{
    public class User
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public decimal Balance { get; set; }
        public decimal? PendingBalance { get; set; }
    }
}