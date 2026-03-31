using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPending;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPending;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingDetails;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingHeader;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetPoPending;


namespace PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry
{
    public interface IGRNEntryQueryRepository
    {
        Task<string> GetDocumentDirectoryAsync();
        Task<List<GetGateEntryPendingPoDto>> GetPendingPoAsync(int partyId);
        Task<List<GetGateEntryPendingDto>> GetPendingPoGateAsync(int partyId, int poId);
        Task<List<GetGrnPendingDto>> GetPendingPoGrnAsync(int partyId, int poId, int gateEntryId);
        Task<List<ValidateToleranceDto>> ValidateToleranceQuantity(int partyId, int poId, int Poslno, int ItemId);
        Task<List<GetGrnPendingDetailsDto>> GetPendingGateEntriesForGrnAsync(int? GrnId, bool? IsGrnGenerated, bool? IsQcGenerated);
        Task<(List<GetGrnPendingHeaderDto>, int)> GetPendingGrnHeaderAsync(DateTimeOffset? fromDate, DateTimeOffset? toDate, bool? IsGrnGenerated, bool? IsQcGenerated, int PageNumber, int PageSize, string? SearchTerm);
        Task<(List<GetGrnQCCompletedDetailsDto>, int)> GetGrnQcCompletedHeader(DateTimeOffset? fromDate, DateTimeOffset? toDate, int PageNumber, int PageSize, string? SearchTerm);
        Task<List<GetGrnQCCompletedDto>> GetGrnQcCompletedDetails(int? GrnId, int? ItemId);
        Task<decimal?> GetUnitPriceAsync(int poId, int itemId, int poSlNoLocal);   
        Task<List<PoValueDetailsDto>> GetPoOtherDetails(int PoId, int PoSlNoLocal,int PoCategoryId, int PoMethodId, int ItemId);
        Task<List<GetPoPendingDto>> GetPoPendingAsync();
    }
}