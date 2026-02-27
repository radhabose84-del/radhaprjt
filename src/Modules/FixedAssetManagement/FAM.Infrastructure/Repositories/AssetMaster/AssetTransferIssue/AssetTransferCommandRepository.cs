using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetTransfer
{
    public class AssetTransferCommandRepository : IAssetTransferCommandRepository
    {

      private readonly ApplicationDbContext _applicationDbContext;

        public AssetTransferCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

      
            public async Task<int> CreateAssetTransferAsync(AssetTransferIssueHdr  assetTransferIssuHdr)
                {
                     var entry =_applicationDbContext.Entry(assetTransferIssuHdr);
                    await _applicationDbContext.AssetTransferIssueHdr.AddAsync(assetTransferIssuHdr);                                        
                    // 🔹 Save changes
                    await _applicationDbContext.SaveChangesAsync();                    
                    // 🔹 Return the generated ID
                    return assetTransferIssuHdr.Id;
                }
                public async Task<AssetTransferIssueDtl> CreateAssetTransferIssueAsync(AssetTransferIssueDtl  assetTransferIssueDtl)
                {

                 // Check if the asset already exists in the transfer issue details table
                        var existingAsset = await _applicationDbContext.AssetTransferIssueDtl
                            .FirstOrDefaultAsync(a => a.AssetId == assetTransferIssueDtl.AssetId);
                     if (existingAsset != null)
                        {
                            throw new Exception($"Asset with ID {assetTransferIssueDtl.AssetId} is already in transfer.");
                        }
                    _applicationDbContext.AssetTransferIssueDtl.Add(assetTransferIssueDtl);
                    await _applicationDbContext.SaveChangesAsync();
                    return assetTransferIssueDtl;
                }


                public async Task<bool> UpdateAssetTransferAsync(AssetTransferIssueHdr assetTransferIssueHdr)
                {
                    // 🔹 Find the existing record
                    var existingRecord = (await _applicationDbContext.AssetTransferIssueHdr
                        .FirstOrDefaultAsync(h => h.Id == assetTransferIssueHdr.Id))!;

                   existingRecord.DocDate = assetTransferIssueHdr.DocDate;
                   existingRecord.TransferType = assetTransferIssueHdr.TransferType;
                   existingRecord.FromUnitId = assetTransferIssueHdr.FromUnitId;
                   existingRecord.ToUnitId = assetTransferIssueHdr.ToUnitId;
                   existingRecord.FromDepartmentId = assetTransferIssueHdr.FromDepartmentId;
                   existingRecord.ToDepartmentId = assetTransferIssueHdr.ToDepartmentId;
                   existingRecord.FromCustodianId = assetTransferIssueHdr.FromCustodianId;
                   existingRecord.ToCustodianId = assetTransferIssueHdr.ToCustodianId;
                   existingRecord.FromCustodianName = assetTransferIssueHdr.FromCustodianName;
                   existingRecord.ToCustodianName = assetTransferIssueHdr.ToCustodianName;
                   existingRecord.GatePassNo=assetTransferIssueHdr.GatePassNo;
                   existingRecord.Status = assetTransferIssueHdr.Status;
                   existingRecord.ModifiedBy = assetTransferIssueHdr.ModifiedBy;
                   existingRecord.ModifiedDate = assetTransferIssueHdr.ModifiedDate;
                   existingRecord.ModifiedIP = assetTransferIssueHdr.ModifiedIP;
                   existingRecord.ModifiedByName = assetTransferIssueHdr.ModifiedByName;

                    _applicationDbContext.AssetTransferIssueHdr.Update(existingRecord);
                    
                    _applicationDbContext.AssetTransferIssueDtl.RemoveRange(_applicationDbContext.AssetTransferIssueDtl.Where(x => x.AssetTransferId == assetTransferIssueHdr.Id));
                   await _applicationDbContext.SaveChangesAsync();
                    
                   await _applicationDbContext.AssetTransferIssueDtl.AddRangeAsync(assetTransferIssueHdr.AssetTransferIssueDtl ?? new List<AssetTransferIssueDtl>());
                   
                   return await _applicationDbContext.SaveChangesAsync()>0;

                    
                }
              
    }   
}