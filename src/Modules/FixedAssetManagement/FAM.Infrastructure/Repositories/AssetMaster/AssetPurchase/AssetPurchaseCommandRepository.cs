using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetPurchase
{
    public class AssetPurchaseCommandRepository : IAssetPurchaseCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public AssetPurchaseCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<int> CreateAsync(AssetPurchaseDetails assetPurchaseDetails)
        {
   
            // Add the AssetPurchaseDetails to the DbContext
        await _applicationDbContext.AssetPurchaseDetails.AddAsync(assetPurchaseDetails);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        // Return the ID of the created AssetPurchaseDetailsId
        return assetPurchaseDetails.Id;
        }

        public async Task<int> UpdateAsync(int Id, AssetPurchaseDetails assetPurchaseDetails)
        {
            var existingassetpurchase = await _applicationDbContext.AssetPurchaseDetails.FirstOrDefaultAsync(u => u.Id == Id);

        // If the assetGroup does not exist
        if (existingassetpurchase is null)
        {
            return -1; //indicate failure
        }

        // Update the existing assetGroup properties
        existingassetpurchase.BudgetType = assetPurchaseDetails.BudgetType;
        existingassetpurchase.VendorCode = assetPurchaseDetails.VendorCode;
        existingassetpurchase.VendorName = assetPurchaseDetails.VendorName;

        existingassetpurchase.PoDate = assetPurchaseDetails.PoDate;
        existingassetpurchase.PoNo = assetPurchaseDetails.PoNo;
        existingassetpurchase.PoSno = assetPurchaseDetails.PoSno;

        existingassetpurchase.ItemCode = assetPurchaseDetails.ItemCode;
        existingassetpurchase.ItemName = assetPurchaseDetails.ItemName;
        existingassetpurchase.GrnNo = assetPurchaseDetails.GrnNo;

        existingassetpurchase.GrnSno = assetPurchaseDetails.GrnSno;
        existingassetpurchase.GrnDate = assetPurchaseDetails.GrnDate;
        existingassetpurchase.QcCompleted = assetPurchaseDetails.QcCompleted;

        existingassetpurchase.AcceptedQty = assetPurchaseDetails.AcceptedQty;
        existingassetpurchase.PurchaseValue = assetPurchaseDetails.PurchaseValue;
        existingassetpurchase.GrnValue = assetPurchaseDetails.GrnValue;

        existingassetpurchase.BillNo = assetPurchaseDetails.BillNo;
        existingassetpurchase.BillDate = assetPurchaseDetails.BillDate;
        existingassetpurchase.Uom = assetPurchaseDetails.Uom;

        existingassetpurchase.BinLocation = assetPurchaseDetails.BinLocation;
        existingassetpurchase.PjYear = assetPurchaseDetails.PjYear;
        existingassetpurchase.PjDocId = assetPurchaseDetails.PjDocId;

        existingassetpurchase.PjDocSr = assetPurchaseDetails.PjDocSr;
        existingassetpurchase.PjDocNo = assetPurchaseDetails.PjDocNo;
        existingassetpurchase.CapitalizationDate = assetPurchaseDetails.CapitalizationDate ?? null;
        
        // Mark the entity as modified
        _applicationDbContext.AssetPurchaseDetails.Update(existingassetpurchase);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
        }
    }
}