using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using OfficeOpenXml;

namespace FAM.Application.ExcelImport
{
    public class AssetAdditionalCostHandler
    {
        private readonly ExcelWorksheet _worksheet;
        private readonly int _row;

        public AssetAdditionalCostHandler(ExcelWorksheet worksheet, int row)
        {
            _worksheet = worksheet;
            _row = row;
        }

        public List<AssetAdditionalCostCombineDto>? ProcessAssetAdditionalCost()
        {
            decimal amount = decimal.TryParse(_worksheet.Cells[_row, 47].Value?.ToString(), out decimal parsedAmount) ? parsedAmount : 0;

            if (amount <= 0) return null;

            return new List<AssetAdditionalCostCombineDto>
            {
                new AssetAdditionalCostCombineDto
                {
                    Amount = amount,
                    JournalNo = string.Empty,
                    CostType = 57,
                    AssetSourceId = 2
                }
            };
        }
    }
}
