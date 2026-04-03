using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeeder;
using MaintenanceManagement.Domain.Entities.Power;

namespace MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder
{
    public interface IFeederQueryRepository
    {
        Task<(List<GetFeederDto>, int)> GetAllFeederAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<Feeder> GetFeederByIdAsync(int id);
        Task<bool> AlreadyExistsAsync(string feederCode, int? id = null);
        Task<bool> NotFoundAsync(int id);

        Task<List<Feeder>> GetFeederAutoComplete(string searchPattern);
        Task<bool> IsFeederLinkedAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);

    }
}