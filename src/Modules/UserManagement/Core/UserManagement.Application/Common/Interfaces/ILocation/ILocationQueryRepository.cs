namespace UserManagement.Application.Common.Interfaces.ILocation
{
    public interface ILocationQueryRepository
    {
        Task<(List<UserManagement.Domain.Entities.Location>, int)> GetAllLocationAsync(int PageNumber, int PageSize, string? SearchTerm);

        Task<UserManagement.Domain.Entities.Location?> GetLocationByIdAsync(int id);

        Task<List<UserManagement.Domain.Entities.Location>> GetAllLocationAsync(string SearchPattern);

        Task<bool> AlreadyExistsAsync(string code, int? id = null);

        Task<bool> NotFoundAsync(int id);
    }
}
