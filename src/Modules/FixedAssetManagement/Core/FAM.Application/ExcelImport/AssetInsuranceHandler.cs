using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using OfficeOpenXml;

namespace FAM.Application.ExcelImport
{
    public class AssetInsuranceHandler
    {
        private readonly ExcelWorksheet _worksheet;
        private readonly int _row;

        public AssetInsuranceHandler(ExcelWorksheet worksheet, int row)
        {
            _worksheet = worksheet;
            _row = row;
        }

        public List<AssetInsuranceCombineDto> ProcessAssetInsurance()
        {
            string? policyNo = _worksheet.Cells[_row, 48].Value?.ToString()?.Trim();
            string? policyAmountStr = _worksheet.Cells[_row, 51].Value?.ToString()?.Trim();
            decimal policyAmount = decimal.TryParse(policyAmountStr, out decimal parsedAmount) ? parsedAmount : 0;
            bool isPolicyValid = policyAmount > 0 && !string.IsNullOrEmpty(policyNo);

            if (!isPolicyValid) return null!;

            return new List<AssetInsuranceCombineDto>
            {
                new AssetInsuranceCombineDto
                {
                    PolicyNo = policyNo,
                    StartDate = DateOnly.TryParse(_worksheet.Cells[_row, 49].Value?.ToString(), out DateOnly startDate) ? (DateOnly?)startDate : null,
                    EndDate = DateOnly.TryParse(_worksheet.Cells[_row, 50].Value?.ToString(), out DateOnly endDate) ? (DateOnly?)endDate : null,
                    PolicyAmount = policyAmount.ToString(),
                    VendorCode = _worksheet.Cells[_row, 52].Value?.ToString()?.Trim() ?? string.Empty
                }
            };
        }
    }
}
