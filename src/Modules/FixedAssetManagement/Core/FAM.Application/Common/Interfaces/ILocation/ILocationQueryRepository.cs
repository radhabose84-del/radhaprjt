using FAM.Application.Location.Queries.GetLocations;

namespace FAM.Application.Common.Interfaces.ILocation
{
    public interface ILocationQueryRepository
    {
        // Task<(List<FAM.Domain.Entities.Location>,int)> GetAllLocationAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<FAM.Domain.Entities.Location> GetByIdAsync(int id);
        Task<List<FAM.Domain.Entities.Location>> GetLocation(string searchPattern);
        Task<FAM.Domain.Entities.Location?> GetByLocationNameAsync(string name, int DepartmentId, int UnitId, int? id = null);
        Task<bool> IsLinkedWithSubLocationsAsync(int locationId);
        Task<(List<LocationDto>,int)> GetAllLocationListAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}