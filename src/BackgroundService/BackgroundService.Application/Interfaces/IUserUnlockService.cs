

namespace BackgroundService.Application.Interfaces
{
    public interface IUserUnlockService
    {        
       public Task UnlockUser(string username);
    }
}