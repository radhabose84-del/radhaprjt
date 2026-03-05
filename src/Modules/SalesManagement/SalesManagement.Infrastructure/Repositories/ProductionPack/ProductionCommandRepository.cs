using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IProductionPack;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.ProductionPack
{
    public class ProductionCommandRepository : IProductionCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public ProductionCommandRepository(
            ApplicationDbContext applicationDbContext,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup,
            IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _applicationDbContext = applicationDbContext;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<string> GenerateNextPackNoAsync(int warehouseId, int binId, CancellationToken ct = default)
        {
            // Get first 3 characters of warehouse code
            var warehouses = await _warehouseLookup.GetByIdsAsync(new[] { warehouseId }, ct);
            var whName = warehouses.FirstOrDefault()?.WarehouseName ?? "WH";
            var whPrefix = (whName.Length >= 3 ? whName[..3] : whName).ToUpper();

            // Get first 3 characters of bin code
            var bins = await _binLookup.GetByIdsAsync(new[] { binId }, ct);
            var binName = bins.FirstOrDefault()?.BinName ?? "BIN";
            var binPrefix = (binName.Length >= 3 ? binName[..3] : binName).ToUpper();

            var prefix = $"PA-{whPrefix}-{binPrefix}-";

            var lastNo = await _applicationDbContext.ProductionPackHeader
                .Where(x => x.PackNo != null && x.PackNo.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.PackNo)
                .FirstOrDefaultAsync(ct);

            int nextSeq = 1;
            if (lastNo != null)
            {
                var parts = lastNo.Split('-');
                if (parts.Length >= 4 && int.TryParse(parts[^1], out var lastSeq))
                {
                    nextSeq = lastSeq + 1;
                }
            }

            return $"{prefix}{nextSeq}";
        }

        public async Task<int> CreateAsync(ProductionPackHeader entity)
        {
            // Step 1: Separate details from header
            var details = entity.ProductionPackDetails?.ToList();
            entity.ProductionPackDetails = null;

            // Step 2: Save header → get HeaderId
            await _applicationDbContext.ProductionPackHeader.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();

            // Step 3: Save details → get each Detail.Id, then generate StockLedger rows
            if (details != null && details.Count > 0)
            {
                foreach (var detail in details)
                {
                    detail.ProductionPackHeaderId = entity.Id;
                }
                await _applicationDbContext.ProductionPackDetail.AddRangeAsync(details);
                await _applicationDbContext.SaveChangesAsync();

                // Step 4: Fetch "Packed" status from MiscMaster
                var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByCode(                    
                     MiscEnumEntity.Packed);
                var packedStatusId = packedStatus?.Id ?? 0;

                // Step 5: Generate StockLedger rows from each detail's pack range
                var stockLedgerEntries = new List<StockLedger>();
                foreach (var detail in details)
                {
                    for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                    {
                        stockLedgerEntries.Add(new StockLedger
                        {
                            UnitId = entity.UnitId,
                            DocType = "PROD",
                            DocNo = entity.Id,
                            DetailDocNo = detail.Id,
                            DocDate = entity.PackDate,
                            ItemId = detail.ItemId,
                            LotId = detail.LotId,
                            PackNo = packNo,
                            PackTypeId = detail.PackTypeId,
                            WarehouseId = entity.WarehouseId,
                            BinId = detail.BinId,
                            TotalQty = 1,
                            TotalValue = detail.NetWeightPerPack,
                            StatusId = packedStatusId
                        });
                    }
                }

                if (stockLedgerEntries.Count > 0)
                {
                    await _applicationDbContext.StockLedger.AddRangeAsync(stockLedgerEntries);
                    await _applicationDbContext.SaveChangesAsync();
                }
            }

            return entity.Id;
        }

        public async Task<int> UpdateAsync(ProductionPackHeader entity)
        {
            var existingEntity = await _applicationDbContext.ProductionPackHeader
                .Include(h => h.ProductionPackDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // Update header fields (StatusId removed from header)
            existingEntity.PackDate = entity.PackDate;
            existingEntity.UnitId = entity.UnitId;
            existingEntity.WarehouseId = entity.WarehouseId;
            existingEntity.TotalBags = entity.TotalBags;
            existingEntity.TotalNetWeight = entity.TotalNetWeight;
            existingEntity.Remarks = entity.Remarks;
            existingEntity.IsActive = entity.IsActive;

            // Remove old StockLedger entries for existing details
            if (existingEntity.ProductionPackDetails != null && existingEntity.ProductionPackDetails.Any())
            {
                var oldStockLedger = await _applicationDbContext.StockLedger
                    .Where(sl => sl.DocType == "PROD" && sl.DocNo == existingEntity.Id)
                    .ToListAsync();

                if (oldStockLedger.Count > 0)
                {
                    _applicationDbContext.StockLedger.RemoveRange(oldStockLedger);
                }

                // Remove existing details
                _applicationDbContext.ProductionPackDetail.RemoveRange(existingEntity.ProductionPackDetails);
            }

            // Insert new details + StockLedger
            if (entity.ProductionPackDetails != null && entity.ProductionPackDetails.Any())
            {
                var newDetails = entity.ProductionPackDetails.ToList();
                foreach (var detail in newDetails)
                {
                    detail.ProductionPackHeaderId = existingEntity.Id;
                }
                await _applicationDbContext.ProductionPackDetail.AddRangeAsync(newDetails);
                await _applicationDbContext.SaveChangesAsync();

                // Fetch "Packed" status from MiscMaster
                var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByCode(
                                        MiscEnumEntity.Packed);
                var packedStatusId = packedStatus?.Id ?? 0;

                // Generate new StockLedger rows
                var stockLedgerEntries = new List<StockLedger>();
                foreach (var detail in newDetails)
                {
                    for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                    {
                        stockLedgerEntries.Add(new StockLedger
                        {
                            UnitId = existingEntity.UnitId,
                            DocType = "PROD",
                            DocNo = existingEntity.Id,
                            DetailDocNo = detail.Id,
                            DocDate = existingEntity.PackDate,
                            ItemId = detail.ItemId,
                            LotId = detail.LotId,
                            PackNo = packNo,
                            PackTypeId = detail.PackTypeId,
                            WarehouseId = existingEntity.WarehouseId,
                            BinId = detail.BinId,
                            TotalQty = 1,
                            TotalValue = detail.NetWeightPerPack,
                            StatusId = packedStatusId
                        });
                    }
                }

                if (stockLedgerEntries.Count > 0)
                {
                    await _applicationDbContext.StockLedger.AddRangeAsync(stockLedgerEntries);
                }
            }

            _applicationDbContext.ProductionPackHeader.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }
    }
}
