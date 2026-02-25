namespace UserManagement.Application.Common.Interfaces.ITimeZones
{
    public interface ITimeZonesCommandRepository
    {
      Task<int> CreateAsync(UserManagement.Domain.Entities.TimeZones timeZones);
      Task<int> UpdateAsync(int Id,UserManagement.Domain.Entities.TimeZones timeZones);
      Task<int> DeleteEntityAsync(int Id,UserManagement.Domain.Entities.TimeZones timeZones);
    }
}