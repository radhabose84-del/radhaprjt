namespace FAM.Application.Dashboard.CardView
{
    public class AssetGroupSummaryDto
    {
        public string GroupName { get; set; } = string.Empty;
        public int AssetCount { get; set; }
        public decimal TotalPurchaseValue { get; set; }
    }
}