namespace UserManagement.Application.Common.Interfaces.IStation
{
    public interface IStationCommandRepository
    {
        Task<int> CreateAsync(UserManagement.Domain.Entities.Station station);

        Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.Station station);

        Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.Station station);
    }
}
