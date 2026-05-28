using PurchaseManagement.Application.VendorEvaluationHeader.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader
{
    public interface IVendorEvaluationHeaderQueryRepository
    {
        Task<(List<VendorEvaluationHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<VendorEvaluationHeaderDto?> GetByIdAsync(int id);
        Task<bool> CompositeKeyExistsAsync(int vendorId, int evaluationMonth, int evaluationYear, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> VendorExistsAsync(int vendorId);
        Task<bool> GradeExistsAsync(int gradeId);
        Task<bool> CriteriaExistsAsync(int criteriaId);
    }
}
