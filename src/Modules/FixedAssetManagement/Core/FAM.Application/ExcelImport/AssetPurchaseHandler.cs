using System.Globalization;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces;
using OfficeOpenXml;

namespace FAM.Application.ExcelImport
{
    public class AssetPurchaseHandler
    {
        private readonly ExcelWorksheet _ws;
        private readonly int _row;
        private readonly IIPAddressService _ip;

        public AssetPurchaseHandler(ExcelWorksheet worksheet, int row, IIPAddressService ipAddressService)
        {
            _ws  = worksheet;
            _row = row;
            _ip  = ipAddressService;
        }

        // If you want to attach a fixed offset to date-only cells (e.g., IST +05:30), set it here.
        // Otherwise, set to null to use local offset.
        private static readonly TimeSpan? DefaultOffset = TimeSpan.FromHours(5.5);

        public List<AssetPurchaseCombineDto> ProcessAssetPurchase()
        {
            var oldUnitId     = _ip.GetOldUnitId();

            // ---- Read cells (adjust column numbers if your sheet differs) ----
            string vendorCode   = GetText(_ws, _row, 36);
            string vendorName   = GetText(_ws, _row, 37);
            int      poNo        = GetInt(_ws, _row, 38);
            var      poDate      = GetDate(_ws, _row, 39, DefaultOffset);
            string  pjYear      = GetText(_ws, _row, 40);
            var      billDate    = GetDate(_ws, _row, 41, DefaultOffset); // Invoice Date
            string  billNo      = GetText(_ws, _row, 42);                // Invoice No
            decimal  purchaseVal = GetDecimal(_ws, _row, 43);             // Cost of purchase
            var      grnDate     = GetDate(_ws, _row, 44, DefaultOffset); // Inward Date/GRN Date if that's your map

            // Skip creating a row if the whole "purchase" area is empty
            bool anyPurchaseData =
                !string.IsNullOrWhiteSpace(vendorCode) ||
                !string.IsNullOrWhiteSpace(vendorName) ||
                poNo > 0 || poDate.HasValue ||
                !string.IsNullOrWhiteSpace(pjYear) ||
                billDate.HasValue || !string.IsNullOrWhiteSpace(billNo) ||
                purchaseVal > 0 || grnDate.HasValue;

            if (!anyPurchaseData)
                return null!;

            // DB requires NOT NULL? force safe defaults:
            var safeVendorCode = string.IsNullOrWhiteSpace(vendorCode) ? "NA" : vendorCode.Trim();
            var safeVendorName = string.IsNullOrWhiteSpace(vendorName) ? "NA" : vendorName.Trim();

            return new List<AssetPurchaseCombineDto>
            {
                new AssetPurchaseCombineDto
                {
                    OldUnitId      = oldUnitId,
                    VendorCode     = safeVendorCode,
                    VendorName     = safeVendorName,
                    PoNo           = poNo,
                    PoDate         = poDate,
                    PjYear         = pjYear ?? string.Empty,
                    BillDate       = billDate,
                    BillNo         = billNo ?? string.Empty,
                    PurchaseValue  = purchaseVal,

                    // Map what you don't have in the sheet to safe defaults
                    GrnNo          = 0,
                    GrnDate        = grnDate,
                    ItemCode       = string.Empty,
                    ItemName       = string.Empty,
                    QcCompleted    = 'Y',
                    AssetSourceId  = 2,
                    AcceptedQty    = 0,
                    BudgetType     = "CAPIT",
                    PjDocId        = string.Empty
                }
            };
        }

        // ----------------- helpers -----------------

        private static string GetText(ExcelWorksheet ws, int row, int col)
        {
            // Prefer .Text (formatted), fall back to .Value
            var s = ws.Cells[row, col].Text;
            if (string.IsNullOrWhiteSpace(s))
                s = ws.Cells[row, col].Value?.ToString();

            s = s?.Trim();
            return string.IsNullOrWhiteSpace(s) ? null! : s;
        }

        private static int GetInt(ExcelWorksheet ws, int row, int col)
        {
            var cell = ws.Cells[row, col];
            var v = cell?.Value;

            if (v == null) return 0;
            if (v is int i) return i;
            if (v is double d) return Convert.ToInt32(Math.Truncate(d));

            var s = (cell!.Text ?? v.ToString() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(s)) return 0;

            // Strip non-digits (commas/currency) then parse
            var digits = new string(s.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0;
        }

        private static decimal GetDecimal(ExcelWorksheet ws, int row, int col)
        {
            var cell = ws.Cells[row, col];
            var v = cell?.Value;

            if (v == null) return 0m;
            if (v is decimal dm) return dm;
            if (v is double dd) return Convert.ToDecimal(dd);

            // Use displayed text (handles Excel number formats)
            var s = (cell!.Text ?? v.ToString() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(s)) return 0m;

            // Handle "10,800" or "1,10,000" and currency symbols
            s = s.Replace(",", "").Replace("₹", "").Trim();

            return decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
                                    CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0m;
        }

        private static DateTimeOffset? GetDate(ExcelWorksheet ws, int row, int col, TimeSpan? offset)
        {
            var cell = ws.Cells[row, col];
            var v = cell?.Value;

            // 1) Already a DateTime
            if (v is DateTime dt) return ToDto(dt, offset);

            // 2) Excel serial number (OADate)
            if (v is double d) return ToDto(DateTime.FromOADate(d), offset);

            // 3) Use formatted text if present
            var s = (cell!.Text ?? v?.ToString() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(s)) return null;

            // exact formats we commonly see (e.g., "2015-05-08")
            string[] formats = { "yyyy-MM-dd", "dd-MM-yyyy", "MM-dd-yyyy", "yyyy/MM/dd", "dd/MM/yyyy", "M/d/yyyy", "d/M/yyyy" };
            if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
                return ToDto(exact, offset);

            // final fallback
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var any))
                return new DateTimeOffset(any);

            return null;
        }

        private static DateTimeOffset ToDto(DateTime dt, TimeSpan? offset)
        {
            var unspecified = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            return offset.HasValue
                ? new DateTimeOffset(unspecified, offset.Value)
                : new DateTimeOffset(unspecified, TimeZoneInfo.Local.GetUtcOffset(unspecified));
        }
    }
}
