using GateEntryManagement.Domain.Entities;

namespace GateEntryManagement.Application.Common.Interfaces.IGateInward
{
    public interface IGateInwardCommandRepository
    {
        Task<int> CreateAsync(GateInwardHdr entity, int transactionTypeId);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);

        // Clears the single attachment columns; returns the old relative file
        // path to delete from disk, or null if none / record not found.
        Task<string?> ClearAttachmentAsync(int gateInwardHdrId, CancellationToken ct);
    }
}
