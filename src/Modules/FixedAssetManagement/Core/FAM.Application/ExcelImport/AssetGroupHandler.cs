using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using OfficeOpenXml;

namespace FAM.Application.ExcelImport
{
    public class AssetGroupHandler
    {
        private readonly IExcelImportCommandRepository _assetRepository;
        private readonly IExcelImportQueryRepository _assetQueryRepository;

        public AssetGroupHandler(IExcelImportCommandRepository assetRepository, IExcelImportQueryRepository assetQueryRepository)
        {
            _assetRepository = assetRepository;
            _assetQueryRepository = assetQueryRepository;
        }

        public async Task<AssetMasterDto> ProcessAssetGroupAsync(ImportAssetCommand request, ExcelWorksheet worksheet, int row)
        {
            var assetDto = new AssetMasterDto();

            // AssetGroup
            string assetGroupName = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty;
            int? assetGroupId = await _assetRepository.GetAssetGroupIdByNameAsync(assetGroupName);
            if (assetGroupId == null)
            {
                throw new Exception($"Invalid Asset Group Name '{assetGroupName}' at Excel Row {row}");
            }
            assetDto.AssetGroupId = assetGroupId.Value;
            // AssetSubGroup
            string assetSubGroupName = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty;
            int? assetSubGroupId = await _assetRepository.GetAssetSubGroupIdByNameAsync(assetSubGroupName);
            if (assetSubGroupId == null)
            {
                //throw new Exception($"Invalid Asset Sub Group Name '{assetSubGroupName}' at Excel Row {row}");
                assetDto.AssetSubGroupId = null; // Allow null for AssetSubGroupId if not found
            }
            else
            {
                assetDto.AssetSubGroupId = assetSubGroupId.Value;
            }
            // AssetCategory
            string assetCategory = worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty;
            int? assetCategoryId = await _assetRepository.GetAssetCategoryIdByNameAsync(assetCategory);
            if (assetCategoryId == null)
            {
                throw new Exception($"Invalid Asset Category Name '{assetCategory}' at Excel Row {row}");
            }
            assetDto.AssetCategoryId = assetCategoryId.Value;

            // AssetSubCategory
            string assetSubCategory = worksheet.Cells[row, 4].Value?.ToString() ?? string.Empty;
            int? assetSubCategoryId = await _assetRepository.GetAssetSubCategoryIdByNameAsync(assetDto.AssetCategoryId ,assetSubCategory);
            if (assetSubCategoryId == null)
            {
                throw new Exception($"Invalid Asset Sub Category Name '{assetSubCategory}' at Excel Row {row}");
            }
            assetDto.AssetSubCategoryId = assetSubCategoryId.Value;

            assetDto.AssetName = worksheet.Cells[row, 6].Value?.ToString();
            assetDto.Quantity = int.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out int quantity) ? quantity : throw new Exception("Invalid Quantity");
            assetDto.Active = bool.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out bool isActive) ? isActive : false;
            assetDto.WorkingStatus = 1;
            assetDto.AssetType = (assetDto.AssetParentId != null && assetDto.AssetParentId > 0) ? 18 : 17;
            assetDto.AssetDescription = worksheet.Cells[row, 5].Value?.ToString() ?? string.Empty;
            assetDto.MachineCode = worksheet.Cells[row, 8].Value?.ToString() ?? string.Empty;
            assetDto.UOMId = await _assetRepository.GetAssetUOMIdByNameAsync(worksheet.Cells[row, 8].Value?.ToString() ?? string.Empty);
            if (assetDto.UOMId == null)
            {
                throw new Exception($"Invalid Asset UOM '{worksheet.Cells[row, 8].Value?.ToString() ?? string.Empty}' at Excel Row {row}");
            }
            return assetDto;
        }
    }
}
