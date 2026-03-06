
namespace BackgroundService.Application
{
   public static class ForgotPasswordCache
    {
        public static readonly Dictionary<string, VerificationCodeDetails> CodeStorage = new();

        public static void RemoveVerificationCode(string username)
        {
            if (CodeStorage.ContainsKey(username))
            {
                CodeStorage.Remove(username);
            }
        }
    }

    public class VerificationCodeDetails
    {
        public string? Code { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}