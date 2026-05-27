using Contracts.Dtos.Lookups.Purchase;
using PurchaseManagement.Application.VendorRatingGrade.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade
{
    public interface IVendorRatingGradeQueryRepository
    {
        Task<(List<VendorRatingGradeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<VendorRatingGradeDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<VendorRatingGradeLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string gradeCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ActionTypeExistsAsync(int actionTypeId);
    }
}
