using Microsoft.EntityFrameworkCore;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Domain.Entities;
using GateEntryManagement.Infrastructure.Data;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.Infrastructure.Repositories.GatePass
{
    public class GatePassCommandRepository : IGatePassCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public GatePassCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(GatePassHdr entity)
        {
            await _applicationDbContext.GatePassHdr.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.GatePassHdr
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.GatePassHdr.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
