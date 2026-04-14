using SalesManagement.Application.MovementTypeConfig.Dto;

namespace SalesManagement.Application.Common.Interfaces.IMovementTypeConfig
{
    public interface IMovementTypeConfigQueryRepository
    {
        Task<(List<MovementTypeConfigDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<MovementTypeConfigDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<MovementTypeConfigLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string movementCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> MiscMasterExistsAsync(int miscMasterId);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsMovementTypeConfigLinkedAsync(int id);
    }
}
