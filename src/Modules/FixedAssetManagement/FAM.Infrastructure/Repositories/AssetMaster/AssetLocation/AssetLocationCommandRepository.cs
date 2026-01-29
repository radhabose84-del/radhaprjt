using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetLocation
{
    public class AssetLocationCommandRepository  : IAssetLocationCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;        

        public AssetLocationCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;            
        }
      

        public async Task<FAM.Domain.Entities.AssetMaster.AssetLocation> CreateAsync(FAM.Domain.Entities.AssetMaster.AssetLocation assetLocation)
        {
            await _applicationDbContext.AssetLocations.AddAsync(assetLocation);
            await _applicationDbContext.SaveChangesAsync();
            return assetLocation;     
        }

        public async Task<int> UpdateAsync(int id, FAM.Domain.Entities.AssetMaster.AssetLocation assetLocation)
        {
            var existingAssetLocation = await _applicationDbContext.AssetLocations.FirstOrDefaultAsync(a => a.AssetId == id);

            if (existingAssetLocation != null)
            {
                // Update relevant fields
                existingAssetLocation.UnitId = assetLocation.UnitId;
                existingAssetLocation.DepartmentId = assetLocation.DepartmentId;
                existingAssetLocation.LocationId = assetLocation.LocationId;
                existingAssetLocation.SubLocationId = assetLocation.SubLocationId;
                existingAssetLocation.CustodianId = assetLocation.CustodianId;
                existingAssetLocation.UserID = assetLocation.UserID;
               

                _applicationDbContext.AssetLocations.Update(existingAssetLocation);
                return await _applicationDbContext.SaveChangesAsync() ;
            }
            
            return 0;
        }


    }
}