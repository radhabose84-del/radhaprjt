namespace UserManagement.Application.Common.Interfaces.ILocation
{
    public interface ILocationCommandRepository
    {
        Task<int> CreateAsync(UserManagement.Domain.Entities.Location location);

        Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.Location location);

        Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.Location location);
    }
}
