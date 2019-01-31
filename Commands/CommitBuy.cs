namespace Titanoboa
{
    public static partial class Commands 
    {
        /*
            CommitBuy command flow:
            1- Get most recent buy (within 60 seconds), 
            2- Remove funds
            3- Add stock amounts
            3- Update buy in transactions table, *set pending flag to false, and update timestamp*
         */
        public static void CommitBuy(string userid) {
            
        }
    }
}