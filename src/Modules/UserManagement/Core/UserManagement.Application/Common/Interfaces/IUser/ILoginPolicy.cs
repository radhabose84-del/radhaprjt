namespace UserManagement.Application.Common.Interfaces.IUser
{
    public interface ILoginPolicy
    {
        Task<string> CanAttemptLogin(string username, DateTime currentTime);
    }
}