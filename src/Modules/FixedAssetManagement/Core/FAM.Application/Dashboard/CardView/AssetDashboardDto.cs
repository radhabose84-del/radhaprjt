namespace FAM.Application.Dashboard.CardView
{
    public class AssetDashboardDto
    {
        public CardViewDto CardView { get; set; } = new();
       public List<AssetGroupSummaryDto> GroupSummary { get; set; } = new();
    }
}