using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.EWaybillHeader
{
    public class EWaybillHeaderCommandRepository : IEWaybillHeaderCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public EWaybillHeaderCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.EWaybillHeader entity)
        {
            await _dbContext.EWaybillHeader.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.EWaybillHeader entity)
        {
            var existing = await _dbContext.EWaybillHeader
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.SupplyType = entity.SupplyType;
            existing.SubSupplyType = entity.SubSupplyType;
            existing.DocumentType = entity.DocumentType;
            existing.TransactionType = entity.TransactionType;
            existing.TotalValue = entity.TotalValue;
            existing.CGST = entity.CGST;
            existing.SGST = entity.SGST;
            existing.IGST = entity.IGST;
            existing.Cess = entity.Cess;
            existing.TransporterId = entity.TransporterId;
            existing.TransporterGSTIN = entity.TransporterGSTIN;
            existing.TransporterName = entity.TransporterName;
            existing.TransportMode = entity.TransportMode;
            existing.TransDocNo = entity.TransDocNo;
            existing.TransDocDate = entity.TransDocDate;
            existing.VehicleNo = entity.VehicleNo;
            existing.VehicleType = entity.VehicleType;
            existing.Distance = entity.Distance;
            existing.PartyId = entity.PartyId;
            existing.EwbStatus = entity.EwbStatus;
            existing.CancelReason = entity.CancelReason;
            existing.IsActive = entity.IsActive;

            _dbContext.EWaybillHeader.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.EWaybillHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.EWaybillHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateAfterNicSuccessAsync(int id, string ewbNumber,
            DateTimeOffset? generatedDate, DateTimeOffset? validUpto, CancellationToken ct = default)
        {
            var existing = await _dbContext.EWaybillHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.EWBNumber = ewbNumber;
            existing.EwbStatus = "Generated";
            existing.GeneratedDate = generatedDate;
            existing.ValidUpto = validUpto;
            existing.ErrorCode = null;
            existing.ErrorMessage = null;

            _dbContext.EWaybillHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateAfterNicFailureAsync(int id, string? errorCode, string? errorMessage,
            CancellationToken ct = default)
        {
            var existing = await _dbContext.EWaybillHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            // EwbStatus stays "Pending" — operator can fix data and retry.
            existing.ErrorCode = errorCode;
            existing.ErrorMessage = errorMessage;

            _dbContext.EWaybillHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
