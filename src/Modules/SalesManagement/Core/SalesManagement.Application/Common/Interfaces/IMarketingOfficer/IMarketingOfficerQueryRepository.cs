using SalesManagement.Application.MarketingOfficer.Dto;

namespace SalesManagement.Application.Common.Interfaces.IMarketingOfficer
{
    public interface IMarketingOfficerQueryRepository
    {
        Task<(List<MarketingOfficerDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<MarketingOfficerDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<MarketingOfficerLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string employeeNo, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SalesOfficeExistsAsync(int salesOfficeId);
        Task<bool> SalesGroupExistsAsync(int salesGroupId);
        Task<bool> SalesGroupsAllExistAsync(List<int> salesGroupIds);
        Task<List<EmployeeLookupDto>> GetEmployeeLookupAsync(string oldUnitId, string? empNo);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsMarketingOfficerLinkedAsync(int id);
    }
}
