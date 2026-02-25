using InventoryManagement.Application.Issue.Queries.GetApprovedMrsById;
using InventoryManagement.Application.Issue.Queries.GetPendingIssue;
using InventoryManagement.Application.Issue.Queries.GetPendingIssueHeader;
using static InventoryManagement.Application.Issue.Queries.GetPendingIssue.GetPendingIssueDto;

namespace InventoryManagement.Application.Common.Interfaces.IIssue
{
    public interface IIssueQueryCommandRepository
    {
         Task<List<GetPendingIssueDto>> GetPendingIssuesAsync(int mrsNo);
        Task<(List<GetPendingIssueHeaderDto>, int)> GetPendingIssueHeaderAsync(DateTimeOffset? fromDate, DateTimeOffset? toDate, int PageNumber, int PageSize, string? SearchTerm);
        Task<string?> GetDescriptionByIdAsync(int id);
        Task<List<GetPendingStockBinDto>> GetMainStoresStockBinWise(
            List<int> itemId,
           int warehouseId);
        Task<List<GetApprovedMrsByIdDto>> GetApprovedMrsDetails(string? searchPattern);
       // Task<(List<PendingIssueReturnDto>, int)> GetPendingIssueReturnAsync(int PageNumber, int PageSize, string? SearchTerm);
        //Task<PendingIssueReturnByIdDto> GetByIdAsync(int id);
    }
}