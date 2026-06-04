namespace UserManagement.Application.Common.Interfaces.IStation
{
    public interface IStationQueryRepository
    {
        Task<(List<UserManagement.Domain.Entities.Station>, int)> GetAllStationAsync(int PageNumber, int PageSize, string? SearchTerm);

        Task<UserManagement.Domain.Entities.Station?> GetStationByIdAsync(int id);

        Task<List<UserManagement.Domain.Entities.Station>> GetAllStationAsync(string SearchPattern);

        Task<bool> AlreadyExistsAsync(string code, int? id = null);

        Task<bool> NotFoundAsync(int id);
    }
}
