using System.Data;
using System.Net;
using AutoMapper;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.ExcelImport.PhysicalStockVerification;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.ExcelImport
{
    public class ExcelImportCommandRepository  : IExcelImportCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;       
        private readonly IMediator _mediator;
        private readonly ILocationCommandRepository _locationCommandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ISubLocationCommandRepository _subLocationCommandRepository;
        
        public ExcelImportCommandRepository(ApplicationDbContext applicationDbContext, IMediator mediator, ILocationCommandRepository locationCommandRepository, IIPAddressService ipAddressService, ISubLocationCommandRepository subLocationCommandRepository)
        {
            _applicationDbContext = applicationDbContext;
            _mediator = mediator;
            _locationCommandRepository = locationCommandRepository;
            _ipAddressService = ipAddressService;
            _subLocationCommandRepository = subLocationCommandRepository;
        }

        public async Task AddRangeAsync(IEnumerable<AssetMasterGenerals> assets)
        {
            await _applicationDbContext.AssetMasterGenerals.AddRangeAsync(assets);
        }

        public async Task SaveChangesAsync()
        {
            await _applicationDbContext.SaveChangesAsync();
        }
        public async Task<bool> ImportAssetsAsync(List<AssetMasterDto> assetDto, CancellationToken cancellationToken)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _applicationDbContext.Database.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        foreach (var assetDto in assetDto)
                        {
                            var command = new CreateAssetMasterGeneralCommand { AssetMaster = assetDto };

                            var response = await _mediator.Send(command, cancellationToken);

                            if (response == null )
                            {
                                throw new Exception($"Error creating asset '{assetDto.AssetName}' at row.");
                            }
                        }

                        await transaction.CommitAsync(cancellationToken);
                        return true;
                    }
                    catch (DbUpdateException dbEx) // 🔹 Capture EF Core Database Errors
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw new Exception($"Database Transaction Error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);          
                        throw new Exception($"Unexpected Error: {ex.Message}");
                    }
                }
            });
        }
        // ✅ CreateAsync - Handles Single Asset Insert
        public async Task<AssetMasterGenerals?> CreateAsync(AssetMasterGenerals asset)
        {
            await _applicationDbContext.AssetMasterGenerals.AddAsync(asset);
            await _applicationDbContext.SaveChangesAsync();
            return asset; 
        }
        public async Task<int?> GetAssetGroupIdByNameAsync(string assetGroupName)
        {
            var trimmedAssetGroupName = assetGroupName?.Trim(); // ✅ Trim input
            var assetGroup = await _applicationDbContext.AssetGroup
                .Where(a => a.GroupName != null && a.GroupName.Trim() == trimmedAssetGroupName && a.IsDeleted == 0) 
                .Select(a => a.Id)
                .FirstOrDefaultAsync();
            return assetGroup == 0 ? null : assetGroup; 
        }
        public async Task<int?> GetAssetSubGroupIdByNameAsync(string assetSubGroupName)
        {
            var trimmedAssetSubGroupName = assetSubGroupName?.Trim(); // ✅ Trim input
            var assetSubGroup = await _applicationDbContext.AssetSubGroup
                .Where(a => a.SubGroupName != null && a.SubGroupName.Trim() == trimmedAssetSubGroupName && a.IsDeleted == 0) 
                .Select(a => a.Id)
                .FirstOrDefaultAsync();
            return assetSubGroup == 0 ? null : assetSubGroup; 
        }

        public async Task<int?> GetAssetCategoryIdByNameAsync(string assetCategoryName)
        {
            var assetCategory = await _applicationDbContext.AssetCategories
            .Where(a => a.CategoryName == assetCategoryName  && a.IsDeleted == 0)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();        
            return assetCategory == 0 ? null : assetCategory;
        }

        public async Task<int?> GetAssetSubCategoryIdByNameAsync(int assetCategoryId,string assetSubCategoryName)
        {
            var assetSubCategory = await _applicationDbContext.AssetSubCategories
            .Where(a => a.SubCategoryName == assetSubCategoryName  && a.IsDeleted == 0 && a.AssetCategoriesId==assetCategoryId) 
            .Select(a => a.Id)
            .FirstOrDefaultAsync();        
            return assetSubCategory == 0 ? null : assetSubCategory; 
        }

        public async Task<int?> GetAssetUOMIdByNameAsync(string assetUOMName)
        {
            var assetUOM = await _applicationDbContext.UOMs
            .Where(a => a.UOMName == assetUOMName  && a.IsDeleted == 0)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();        
            return assetUOM == 0 ? null : assetUOM;
        }

        public async Task<int?> GetAssetLocationIdByNameAsync(string locationName)
        {
/*             locationName=locationName.Trim();
            var assetLocation = await _applicationDbContext.Locations
            .Where(a => a.LocationName == locationName  && a.IsDeleted == 0)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();        
            return assetLocation == 0 ? null : assetLocation;  */            

            if (string.IsNullOrWhiteSpace(locationName))
                return null;

            locationName = locationName.Trim();

            // Try to find the location
            var assetLocationId = await _applicationDbContext.Locations
                .Where(a => a.LocationName == locationName && a.IsDeleted == 0)
                .Select(a => a.Id)
                .FirstOrDefaultAsync();

            if (assetLocationId != 0)
                return assetLocationId;

            // Location not found – create it
            var newLocation = new Location
            {
                LocationName = locationName,
                Code = GenerateLocationCode(locationName), 
                Description="Excel Import",                
                UnitId=_ipAddressService.GetUnitId(),
                DepartmentId=0,
                IsDeleted = 0,
                CreatedBy=_ipAddressService.GetUserId(),
                CreatedByName=_ipAddressService.GetUserName(),
                CreatedIP=_ipAddressService.GetSystemIPAddress(),
                CreatedDate=DateTime.UtcNow
            };
            // Reuse CreateAsync from your repository (assumes DI of _locationCommandRepository)
            var createdLocation = await _locationCommandRepository.CreateAsync(newLocation);
            return createdLocation.Id > 0 ? createdLocation.Id : null;
        }
        private string GenerateLocationCode(string locationName)
        {
            return $"LOC-{DateTime.UtcNow.Ticks % 10000}";
        }

        public async Task<int?> GetAssetSubLocationIdByNameAsync(string subLocationName,string locationName)
        {
            if (string.IsNullOrWhiteSpace(subLocationName))
                return null;

            subLocationName = subLocationName.Trim();


            locationName = locationName.Trim();

            // Try to find the location
            var assetLocationId = await _applicationDbContext.Locations
                .Where(a => a.LocationName == locationName && a.IsDeleted == 0)
                .Select(a => a.Id)
                .FirstOrDefaultAsync();


            // Check if subLocation already exists (case-insensitive match)
            var subLocationId = await _applicationDbContext.SubLocations
                .Where(s => s.SubLocationName == subLocationName && s.IsDeleted == 0)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (subLocationId != 0)
                return subLocationId;

            // If not found, create new SubLocation
            var newSubLocation = new FAM.Domain.Entities.SubLocation
            {
                SubLocationName = subLocationName,
                Code = GenerateSubLocationCode(subLocationName),
                Description="Excel Import",                
                LocationId = assetLocationId,
                DepartmentId = 0,
                UnitId = _ipAddressService.GetUnitId(),
                IsDeleted = 0,
                CreatedBy=_ipAddressService.GetUserId(),
                CreatedByName=_ipAddressService.GetUserName(),
                CreatedIP=_ipAddressService.GetSystemIPAddress(),
                CreatedDate=DateTime.UtcNow
            };

            var created = await _subLocationCommandRepository.CreateAsync(newSubLocation);
            return created.Id > 0 ? created.Id : null;
        }
        private string GenerateSubLocationCode(string name)
        {
            return $"SUB-{DateTime.UtcNow.Ticks % 10000}";
        }
        public async Task<int?> GetAssetIdByNameAsync(string assetCode)
        {
            var assetMaster = await _applicationDbContext.AssetMasterGenerals
            .Where(a => a.AssetCode == assetCode  && a.IsDeleted == 0)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();        
            return assetMaster == 0 ? null : assetMaster;
        }
        public async Task<int?> GetManufacturerIdByNameAsync(string manufacture)
        {
            var assetManufacturer = await _applicationDbContext.Manufactures
            .Where(a => a.ManufactureName == manufacture  && a.IsDeleted == 0)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();        
            return assetManufacturer == 0 ? null : assetManufacturer; 
        }

        public async Task<bool> BulkInsertAsync(List<AssetAudit> audits, CancellationToken cancellationToken)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var unitId = _ipAddressService.GetUnitId();
                    // Get the current max for that unit
                    var maxUploadId = (await _applicationDbContext.AssetAudit
                        .Where(x => x.UnitId == unitId)
                        .MaxAsync(x => (int?)x.UploadedFileId)) ?? 0;

                    var newUploadId = maxUploadId + 1;

                    // Assign UnitId and UploadedFileId to all items
                    audits.ForEach(a =>
                    {
                        a.UnitId = unitId;
                        a.UploadedFileId = newUploadId;
                    });

                    await _applicationDbContext.AssetAudit.AddRangeAsync(audits, cancellationToken);
                    await _applicationDbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return true;
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw new Exception($"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw new Exception($"Unexpected error: {ex.Message}");
                }
            });
        }

        public async Task<bool> CheckFileExistsAsync(string fileName, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.AssetAudit
            .AnyAsync(x => x.SourceFileName.ToLower() == fileName.ToLower(), cancellationToken);
        }

        public async Task<bool> InsertScannedAssetAsync(AssetAudit entity, CancellationToken cancellationToken)
        {
             var unitId = _ipAddressService.GetUnitId();
            // Assign UnitId to the entity
             entity.UnitId = unitId;
             await _applicationDbContext.AssetAudit.AddAsync(entity, cancellationToken);
             return await _applicationDbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        public async Task<bool> IsAssetAlreadyScannedAsync(string assetCode, int auditCycle, string auditFinancialYear, string department,string unitName ,CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId();
            return await _applicationDbContext.AssetAudit.AnyAsync(x =>
                            x.AssetCode == assetCode.Trim() &&
                            x.AuditTypeId == auditCycle &&
                            x.AuditFinancialYear == auditFinancialYear &&
                            x.Department == department.Trim() &&
                            x.UnitName == unitName.Trim() &&
                            x.UnitId == unitId,
                            cancellationToken);
        }
    }
}
