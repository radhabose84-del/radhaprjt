using System.Data;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using PurchaseManagement.Domain.Entities.GRN.StockLedger;
using Microsoft.EntityFrameworkCore.Storage;

namespace PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry
{
    public interface IGRNEntryCommandRepository
    {
        Task<string> GenerateNextCodeAsync(CancellationToken ct = default);
        Task<int> CreateAsync(GrnHeader grnHeader);
        Task<bool> UpdateAsync(int Id, GrnHeader grnHeader);
        Task<bool> UpdateAsync(int id, GrnHeader grnHeader,List<CalculatedDetail> calculatedDetails, List<UpdateGRNEntryDto.UpdateGRNDetailsDto> detailDtos);
        Task<int> CreatePutawayListAsync(List<GrnPutAwayRule> putawayList);
        Task<int> CreatePutawayListAsync(List<GrnPutAwayRule> putawayList, IDbTransaction transaction);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> CreateStockLedgerEntriesAsync(List<StockLedger> stockLedgerList);
        Task<int> CreatePutawayWithStockLedgerAsync(List<GrnPutAwayRule> putawayList, List<StockLedger> stockLedgerList, Func<Task> publishEvents);
        Task<GrnHeader?> GetGrnHeaderAsync(int grnId, CancellationToken ct = default);
        Task<List<GrnDetail>> GetGrnDetailsByGrnIdAsync(int grnId, CancellationToken ct = default);
        Task<GrnHeader?> GetGrnWithDetailsAsync(int grnId, CancellationToken ct = default);
    }
}