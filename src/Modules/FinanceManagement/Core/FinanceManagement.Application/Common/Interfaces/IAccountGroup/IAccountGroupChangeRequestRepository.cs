using FinanceManagement.Domain.Entities;

namespace FinanceManagement.Application.Common.Interfaces.IAccountGroup
{
    public interface IAccountGroupChangeRequestRepository
    {
        // Adds the pending request to the context WITHOUT saving, so it commits in the same
        // transaction as the outbox CreateApprovalRequestCommand (atomic raise).
        Task AddWithoutSaveAsync(Domain.Entities.AccountGroupChangeRequest changeRequest, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        // Latest pending request for a group (consumer applies it on approval).
        Task<Domain.Entities.AccountGroupChangeRequest?> GetPendingByAccountGroupAsync(int accountGroupId, CancellationToken cancellationToken = default);

        Task MarkStatusAsync(int id, string status, CancellationToken cancellationToken = default);
    }
}
