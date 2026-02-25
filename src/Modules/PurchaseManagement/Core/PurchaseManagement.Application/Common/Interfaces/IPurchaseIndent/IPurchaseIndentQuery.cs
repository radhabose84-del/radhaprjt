// using Contracts.Dtos.Purchase;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndent;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent
{
    public interface IPurchaseIndentQuery
    {
        //Task<(List<IndentHeader>, int)> GetAllPurchaseIndentAsync(int PageNumber, int PageSize, string? SearchTerm,int? StatusId);
        Task<(List<PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto> Items, int TotalCount)> GetAllPurchaseIndentAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        int? statusId
    );
        Task<bool> NotFoundAsync(int id);
        Task<IndentHeader> GetByIdAsync(int id);
        // Task<string> GeneratePurchaseIndentNumberAsync(int unitId);
    //   //  Task<(List<IndentHeader>, int)> GetPendingPurchaseIndentAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<(List<PendingIndentDto>, int)> GetPendingPurchaseIndentAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<IndentHeader>> GetPurchaseIndentAutoCompleteAsync(string Status, string? SearchTerm,bool AllIndents=false);        
        Task<List<IndentForPODto>> GetApprovedIndentDetailsForPO(int? vendorId,int? departmentId, CancellationToken ct = default);

    }
}