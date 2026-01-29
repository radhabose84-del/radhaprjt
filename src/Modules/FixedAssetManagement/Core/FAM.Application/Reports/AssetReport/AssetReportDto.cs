using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.Reports.AssetReport
{
    public class AssetReportDto
    {
        public string? CompanyName { get; set; }
        public string? UnitName { get; set; }
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; }
        public string? GroupName { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? SubLocation { get; set; }
        public string? CustodianName { get; set; }
        public string? UserName { get; set; }
        public string? ParentAssetName { get; set; }
        public string? GRNNumber { get; set; }
        public string? GRNValue { get; set; }
        public decimal PurchaseValue { get; set; }
        public decimal AdditionalCost { get; set; }
        public string? BillNumber { get; set; }
        public string? BillDate { get; set; }
        public DateTimeOffset? CapitalizationDate { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int PoNumber { get; set; }
        public string? VendorName { get; set; }
        public string? UOM { get; set; }
        public decimal TotalAdditionalCost { get; set; }
        public decimal TotalPurchaseValue { get; set; }
    }
}