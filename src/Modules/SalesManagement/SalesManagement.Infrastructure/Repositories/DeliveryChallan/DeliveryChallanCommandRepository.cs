using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.DeliveryChallan
{
    public class DeliveryChallanCommandRepository : IDeliveryChallanCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DeliveryChallanCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNextDeliveryNumberAsync(int fromPlantId, CancellationToken ct = default)
        {
            var prefix = $"DC-{fromPlantId}-";

            var lastNumber = await _dbContext.DeliveryChallanHeader
                .Where(x => x.DeliveryNumber != null && x.DeliveryNumber.StartsWith(prefix))
                .OrderByDescending(x => x.DeliveryNumber)
                .Select(x => x.DeliveryNumber)
                .FirstOrDefaultAsync(ct);

            var nextSeq = 1;
            if (lastNumber != null)
            {
                var seqPart = lastNumber.Substring(prefix.Length);
                if (int.TryParse(seqPart, out var lastSeq))
                {
                    nextSeq = lastSeq + 1;
                }
            }

            return $"{prefix}{nextSeq:D5}";
        }

        public async Task<int> CreateAsync(Domain.Entities.DeliveryChallanHeader entity)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Separate details from header
            var details = entity.DeliveryChallanDetails?.ToList();
            entity.DeliveryChallanDetails = null;

            // Save header first to get auto-generated Id
            await _dbContext.DeliveryChallanHeader.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            // Insert details with header FK
            if (details != null && details.Count > 0)
            {
                foreach (var detail in details)
                {
                    var newDetail = new Domain.Entities.DeliveryChallanDetail
                    {
                        DeliveryChallanHeaderId = entity.Id,
                        StoDetailId = detail.StoDetailId,
                        ItemId = detail.ItemId,
                        LotId = detail.LotId,
                        StartPackNo = detail.StartPackNo,
                        EndPackNo = detail.EndPackNo,
                        DispatchQuantity = detail.DispatchQuantity,
                        UOMId = detail.UOMId,
                        BagCount = detail.BagCount,
                        BaleCount = detail.BaleCount,
                        NetWeight = detail.NetWeight,
                        GrossWeight = detail.GrossWeight,
                        ExMillRate = detail.ExMillRate,
                        LineMovementValue = detail.LineMovementValue
                    };
                    await _dbContext.DeliveryChallanDetail.AddAsync(newDetail);
                }

                await _dbContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return entity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.DeliveryChallanHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.DeliveryChallanHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
