using GateEntryManagement.Domain.Entities;

namespace GateEntryManagement.Application.Common.Interfaces.IGatePass
{
    public interface IGatePassCommandRepository
    {
        Task<int> CreateAsync(GatePassHdr entity, int transactionTypeId);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
