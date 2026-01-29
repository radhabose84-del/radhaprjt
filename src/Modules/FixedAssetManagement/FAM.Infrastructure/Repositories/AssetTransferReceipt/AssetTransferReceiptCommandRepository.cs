using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetTransferReceipt
{
    public class AssetTransferReceiptCommandRepository : IAssetTransferReceiptCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AssetTransferReceiptCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(AssetTransferReceiptHdr assetTransferReceiptHdr,List<FAM.Domain.Entities.AssetMaster.AssetLocation> assetLocation)
        {
             int resultId;
           // Check if AssetTransferId exists in AssetTransferReceiptHdr
                var existingReceipt = await _applicationDbContext.AssetTransferReceiptHdr
                .FirstOrDefaultAsync(x => x.AssetTransferId == assetTransferReceiptHdr.AssetTransferId);

            if (existingReceipt is null)
            {

                // Insert into AssetTransferReceiptHdr table
                var entry =_applicationDbContext.Entry(assetTransferReceiptHdr);
                
                await _applicationDbContext.AssetTransferReceiptHdr.AddAsync(assetTransferReceiptHdr);
            }
            else
            {        
            // Fetch AssetIds where AckStatus = 0
            var assetIds = await _applicationDbContext.AssetTransferReceiptDtl
                .Where(d => d.AssetReceiptId == existingReceipt.Id && d.AckStatus == 0)
                .Select(d => d.AssetId)
                .ToListAsync();

            // Fetch only relevant existing details
            var existingDetails = await _applicationDbContext.AssetTransferReceiptDtl
                .Where(d => d.AssetReceiptId == existingReceipt.Id && assetIds.Contains(d.AssetId))
                .ToListAsync();

            // Convert new details to a dictionary using Tuple as key
            var newDetailsDict = assetTransferReceiptHdr.AssetTransferReceiptDtl
                .ToDictionary(a => (a.AssetId)); // Use AssetId since AssetReceiptId is already known (existingReceipt.Id)

            // Update existing details using LINQ
            var updatedDetails = existingDetails
                .Where(d => newDetailsDict.ContainsKey(d.AssetId))
                .Select(d =>
                {
                    var newDetail = newDetailsDict[d.AssetId];
                    d.AckStatus = newDetail.AckStatus;
                    d.AckDate = newDetail.AckStatus == 1 ? DateTimeOffset.UtcNow : (DateTimeOffset?)null;
                    d.LocationId = newDetail.LocationId;
                    d.SubLocationId = newDetail.SubLocationId;
                    d.UserID = newDetail.UserID;
                    d.UserName=newDetail.UserName;
                    return d;
                })
                .ToList();

                // Apply the updates
                _applicationDbContext.AssetTransferReceiptDtl.UpdateRange(updatedDetails);
  
            }

            // Filter only assets where AckStatus == 1
           var acknowledgedAssets = assetTransferReceiptHdr.AssetTransferReceiptDtl
            .Where(a => a.AckStatus == 1)
            .ToList();

            if (acknowledgedAssets.Any()) // Proceed only if there are acknowledged assets
            {
                var assetIds = acknowledgedAssets.Select(a => a.AssetId).ToList();

                // Update AssetLocations for acknowledged assets using LINQ Join
                var assetLocationsToUpdate = await _applicationDbContext.AssetLocations
                    .Where(x => assetIds.Contains(x.AssetId))
                    .ToListAsync();

                var updatedAssetLocations = assetLocationsToUpdate.Join(
                    assetLocation,
                    existing => existing.AssetId,
                    incoming => incoming.AssetId,
                    (existing, incoming) =>
                    {
                        existing.LocationId = incoming.LocationId;
                        existing.SubLocationId = incoming.SubLocationId;
                        existing.UserID = incoming.UserID;
                        existing.UnitId = incoming.UnitId;
                        existing.CustodianId = incoming.CustodianId;
                        existing.DepartmentId = incoming.DepartmentId;
                        return existing;
                    }).ToList();

                _applicationDbContext.AssetLocations.UpdateRange(updatedAssetLocations);

                // Update FixedAsset.AssetMaster's UnitId using LINQ Join
                var assetMasterRecords = await _applicationDbContext.AssetMasterGenerals
                    .Where(x => assetIds.Contains(x.Id))
                    .ToListAsync();

                var updatedAssetMasters = assetMasterRecords.Join(
                    assetLocation,
                    existing => existing.Id,
                    incoming => incoming.AssetId,
                    (existing, incoming) =>
                    {
                        existing.UnitId = incoming.UnitId;
                        return existing;
                    }).ToList();

                _applicationDbContext.AssetMasterGenerals.UpdateRange(updatedAssetMasters);

                 
            }
           // Save all changes in one transaction for efficiency
            await _applicationDbContext.SaveChangesAsync();
            resultId = existingReceipt?.Id ?? assetTransferReceiptHdr.Id;
            return resultId;
        }
           
            
    }

    }
