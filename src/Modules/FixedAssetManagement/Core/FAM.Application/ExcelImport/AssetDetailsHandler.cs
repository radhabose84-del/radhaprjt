using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using OfficeOpenXml;
using Contracts.Interfaces;
using FAM.Application.Common.Interfaces;

namespace FAM.Application.ExcelImport
{
    public class AssetDetailsHandler
    {
        private readonly IExcelImportCommandRepository _assetRepository;
        private readonly IExcelImportQueryRepository _assetQueryRepository;
        private readonly IIPAddressService _ipAddressService;

        public AssetDetailsHandler(IExcelImportCommandRepository assetRepository, IExcelImportQueryRepository assetQueryRepository, IIPAddressService ipAddressService)
        {
            _assetRepository = assetRepository;
            _assetQueryRepository = assetQueryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task ProcessAssetDetailsAsync(ImportAssetCommand request, ExcelWorksheet worksheet, int row, AssetMasterDto assetDto)
        {
            // Extract Asset Parent
            string assetParent = worksheet.Cells[row, 10].Value?.ToString() ?? string.Empty;
            assetDto.AssetParentId = string.IsNullOrEmpty(assetParent) ? null : await _assetRepository.GetAssetIdByNameAsync(assetParent);

            var companyId = _ipAddressService.GetCompanyId() ?? 0;
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            assetDto.UnitId= unitId;
            assetDto.CompanyId = companyId;    
            
            // Extract Location Details
            int? locationId = await _assetRepository.GetAssetLocationIdByNameAsync(worksheet.Cells[row, 13].Value?.ToString() ?? "");
            int? subLocationId = await _assetRepository.GetAssetSubLocationIdByNameAsync(worksheet.Cells[row, 14].Value?.ToString() ?? "",worksheet.Cells[row, 13].Value?.ToString() ?? "");

            int custodianId = int.TryParse(worksheet.Cells[row, 15].Value?.ToString(), out int parsedCustodianId) ? parsedCustodianId : 0;
            string assetDeptName = worksheet.Cells[row, 12].Value?.ToString() ?? string.Empty;
            int? assetDeptId = await _assetQueryRepository.GetAssetDeptIdByNameAsync(assetDeptName);

            assetDto.AssetLocation = new AssetLocationCombineDto
            {
                UnitId = unitId,
                LocationId = locationId ?? 1,
                SubLocationId = subLocationId ?? 2,
                CustodianId = custodianId,
                DepartmentId = assetDeptId ?? 1
            };
        }
    }
}
