using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static partial class Commands
    {
        public static void CancelSetBuy(string username, JObject commandParams) {
            ParamHelper.ValidateParamsExist(commandParams, "stock");

            //Unpack JObject
            var stockSymbol = commandParams["stock"].ToString();

            //Get user
            var user = TransactionHelper.GetUser(username);

            // Get trigger to cancel
            var existingSetBuyTrigger = TransactionHelper.GetTrigger(user, stockSymbol, "BUY_TRIGGER");
            var refund = existingSetBuyTrigger.BalanceChange;

            // Refund user
            var newBalance = user.Balance + refund;
            TransactionHelper.UpdateUserBalance(ref user, newBalance);

            // Cancel transaction
            TransactionHelper.CancelTransaction(existingSetBuyTrigger);
        }
        
    }
}