using PurchaseManagement.Domain.Entities.MRS;

namespace PurchaseManagement.Application.Common.Interfaces.IMRS
{
    public interface IMrsEntryCommandRepository
    {
        Task<string> GenerateNextCodeAsync(CancellationToken ct = default);
        Task<MrsHeader> CreateAsync(MrsHeader mrsHeader);
        Task<bool> UpdateAsync(MrsHeader mrsHeader);
        Task<bool> UpdateMrsApproveAsync(int id, int statusId, CancellationToken ct = default);    
    }
}