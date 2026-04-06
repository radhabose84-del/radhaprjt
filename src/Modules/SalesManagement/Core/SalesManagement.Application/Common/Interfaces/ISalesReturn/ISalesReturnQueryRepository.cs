using SalesManagement.Application.SalesReturn.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesReturn
{
    public interface ISalesReturnQueryRepository
    {
        Task<(List<SalesReturnListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesReturnHeaderDto?> GetByIdAsync(int id);
        Task<SalesReturnHeaderDto?> GetByComplaintIdAsync(int complaintHeaderId);
        Task<ComplaintReturnDataDto?> GetComplaintReturnDataAsync(int complaintHeaderId);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ComplaintExistsAsync(int complaintHeaderId);
        Task<bool> IsComplaintReturnEligibleAsync(int complaintHeaderId);
        Task<bool> PackRangeOverlapsAsync(int invoiceDetailId, int startPackNo, int endPackNo, int? excludeReturnHeaderId = null);
        Task<bool> PackRangeExistsInDispatchAsync(int invoiceDetailId, int startPackNo, int endPackNo);
        Task<(int TotalDispatchedPacks, int TotalReturnedPacks)> GetReturnProgressAsync(int complaintHeaderId);
    }
}
