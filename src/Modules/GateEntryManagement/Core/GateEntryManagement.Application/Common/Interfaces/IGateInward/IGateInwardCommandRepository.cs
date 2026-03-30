using GateEntryManagement.Domain.Entities;

namespace GateEntryManagement.Application.Common.Interfaces.IGateInward
{
    public interface IGateInwardCommandRepository
    {
        Task<int> CreateAsync(GateInwardHdr entity, int transactionTypeId);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
