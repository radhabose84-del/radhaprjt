using Contracts.Common;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.ItemPriceMaster
{
    public class ItemPriceMasterCommandRepository : IItemPriceMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public ItemPriceMasterCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(Domain.Entities.ItemPriceMaster entity, int typeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    await _applicationDbContext.ItemPriceMaster.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(typeId, dbConnection, dbTransaction);

                    await transaction.CommitAsync();
                    return entity.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<int>> CreateBulkAsync(List<Domain.Entities.ItemPriceMaster> entities, int typeId)
        {
            var newIds = new List<int>();

            foreach (var entity in entities)
            {
                // Generate unique PriceCode for each entity
                var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId);
                var priceCode = sequences.Count > 0 ? sequences[^1] : null;
                entity.PriceCode = priceCode
                    ?? throw new ExceptionRules("No document sequence configured for PriceMaster.");

                var id = await CreateAsync(entity, typeId);
                newIds.Add(id);
            }

            return newIds;
        }

        public async Task<int> UpdateAsync(Domain.Entities.ItemPriceMaster entity)
        {
            var existingEntity = await _applicationDbContext.ItemPriceMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.ItemId = entity.ItemId;
            existingEntity.VariantId = entity.VariantId;
            existingEntity.SalesSegmentId = entity.SalesSegmentId;
            existingEntity.BaseRate = entity.BaseRate;
            existingEntity.TolerancePercentage = entity.TolerancePercentage;
            existingEntity.CharityValue = entity.CharityValue;
            existingEntity.HandlingCharges = entity.HandlingCharges;
            existingEntity.AdditionalValue = entity.AdditionalValue;
            existingEntity.CurrencyId = entity.CurrencyId;
            existingEntity.ValidFrom = entity.ValidFrom;
            existingEntity.ValidTo = entity.ValidTo;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.ItemPriceMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.ItemPriceMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.ItemPriceMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
