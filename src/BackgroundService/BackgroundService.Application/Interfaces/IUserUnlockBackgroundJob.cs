
namespace BackgroundService.Application.Interfaces
{
    public interface IUserUnlockBackgroundJob
    {
       public Task Execute(string userName);
    }
}