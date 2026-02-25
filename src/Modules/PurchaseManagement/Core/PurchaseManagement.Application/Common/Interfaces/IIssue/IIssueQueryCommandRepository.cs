using PurchaseManagement.Application.Issue.Queries.GetApprovedMrsById;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssue;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssueHeader;
using PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturn;
using PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById;
// using MassTransit.Futures.Contracts;
using static PurchaseManagement.Application.Issue.Queries.GetPendingIssue.GetPendingIssueDto;

namespace PurchaseManagement.Application.Common.Interfaces.IIssue
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
        Task<(List<PendingIssueReturnDto>, int)> GetPendingIssueReturnAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<PendingIssueReturnByIdDto> GetByIdAsync(int id);
    }
}