
using SalesManagement.Application.BusinessUnit.Dto;

namespace SalesManagement.Application.Common.Interfaces.IBusinessUnit
{
    public interface IBusinessUnitQueryRepository
    {
        Task<(List<BusinessUnitDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<BusinessUnitDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<BusinessUnitLookupDto>> AutocompleteAsync(string term, CancellationToken cancellationToken);
        Task<bool> AlreadyExistsAsync(string businessUnitCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}
