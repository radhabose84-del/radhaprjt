using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Application.SalesQuotation.Queries.GetPendingSalesQuotationAmendment;

namespace SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment
{
    public interface ISalesQuotationAmendmentQueryRepository
    {
        Task<(List<SalesQuotationAmendmentHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesQuotationAmendmentHeaderDto?> GetByIdAsync(int id);
        Task<List<SalesQuotationAmendmentHeaderDto>> GetBySalesQuotationHeaderIdAsync(int salesQuotationHeaderId);
        Task<(List<PendingSalesQuotationAmendmentDto>, int)> GetPendingAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<bool> SalesQuotationExistsAndApprovedAsync(int salesQuotationHeaderId);
        Task<bool> HasPendingAmendmentAsync(int salesQuotationHeaderId);
        Task<bool> SalesQuotationDetailExistsAsync(int salesQuotationDetailId, int salesQuotationHeaderId);
    }
}
