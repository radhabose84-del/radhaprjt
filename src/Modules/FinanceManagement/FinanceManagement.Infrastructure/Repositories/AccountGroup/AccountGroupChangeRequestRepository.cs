using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Repositories.AccountGroup
{
    public class AccountGroupChangeRequestRepository : IAccountGroupChangeRequestRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AccountGroupChangeRequestRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddWithoutSaveAsync(Domain.Entities.AccountGroupChangeRequest changeRequest, CancellationToken cancellationToken = default)
        {
            await _dbContext.Set<Domain.Entities.AccountGroupChangeRequest>().AddAsync(changeRequest, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Domain.Entities.AccountGroupChangeRequest?> GetPendingByAccountGroupAsync(int accountGroupId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Domain.Entities.AccountGroupChangeRequest>()
                .Where(x => x.AccountGroupId == accountGroupId
                            && x.RequestStatus == MiscEnumEntity.Pending
                            && x.IsDeleted == BaseEntity.IsDelete.NotDeleted)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task MarkStatusAsync(int id, string status, CancellationToken cancellationToken = default)
        {
            var existing = await _dbContext.Set<Domain.Entities.AccountGroupChangeRequest>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (existing == null)
                return;

            existing.RequestStatus = status;
            _dbContext.Update(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
