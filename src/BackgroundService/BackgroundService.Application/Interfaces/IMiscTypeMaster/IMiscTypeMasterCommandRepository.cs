namespace BackgroundService.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository
    {
        Task<Domain.Entities.Notification.MiscTypeMaster> CreateAsync(Domain.Entities.Notification.MiscTypeMaster miscTypeMaster);   
        Task<bool> UpdateAsync(int id, Domain.Entities.Notification.MiscTypeMaster miscTypeMaster);
        Task<bool> DeleteAsync(int id,Domain.Entities.Notification.MiscTypeMaster miscTypeMaster); 
    }
}