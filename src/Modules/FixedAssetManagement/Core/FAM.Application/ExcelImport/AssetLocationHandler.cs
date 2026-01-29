using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using OfficeOpenXml;

namespace FAM.Application.ExcelImport
{
    public class AssetLocationHandler
    {
        private readonly IExcelImportCommandRepository _assetRepository;
        private readonly IExcelImportQueryRepository _assetQueryRepository;

        public AssetLocationHandler(IExcelImportCommandRepository assetRepository, IExcelImportQueryRepository assetQueryRepository)
        {
            _assetRepository = assetRepository;
            _assetQueryRepository = assetQueryRepository;
        }

        public async Task<AssetLocationCombineDto> ProcessLocationAsync(ExcelWorksheet worksheet, int row)
        {
            var assetLocationDto = new AssetLocationCombineDto();

            // Asset Location
            string assetLocationName = worksheet.Cells[row, 13].Value?.ToString() ?? string.Empty;
            if (assetLocationName != ""){
            int? assetLocationId = await _assetRepository.GetAssetLocationIdByNameAsync(assetLocationName);       
            assetLocationDto.LocationId = assetLocationId.Value;
            }else{assetLocationDto.LocationId =1;}
            // Asset SubLocation
            string assetSubLocationName = worksheet.Cells[row, 14].Value?.ToString() ?? string.Empty;
            if (assetLocationName != ""){
            int? assetSubLocationId = await _assetRepository.GetAssetSubLocationIdByNameAsync(assetSubLocationName,assetLocationName);
            /*     if (assetSubLocationId == null)
            {
                throw new Exception($"Invalid Asset SubLocation Name '{assetSubLocationName}' at Excel Row {row}");
            } */
            assetLocationDto.SubLocationId = assetSubLocationId.Value;
            }else{assetLocationDto.SubLocationId =2;}
            // Asset Department
            string assetDeptName = worksheet.Cells[row, 12].Value?.ToString() ?? string.Empty;
            int? assetDeptId = await _assetQueryRepository.GetAssetDeptIdByNameAsync(assetDeptName);
            if (assetDeptId == null)
            {
                throw new Exception($"Invalid Asset Department Name '{assetDeptName}' at Excel Row {row}");
            }
            assetLocationDto.DepartmentId = assetDeptId.Value;

            // CustodianId (from column 15)
            int custodianId = int.TryParse(worksheet.Cells[row, 15].Value?.ToString(), out int parsedCustodianId) ? parsedCustodianId : 0;
            assetLocationDto.CustodianId = custodianId;

            // Check if any of the values are valid to populate the AssetLocationDto
            if (assetLocationDto.LocationId > 0 || assetLocationDto.SubLocationId > 0 || assetLocationDto.DepartmentId > 0 || assetLocationDto.CustodianId > 0)
            {
                return assetLocationDto;
            }
            return null;
        }
    }
}