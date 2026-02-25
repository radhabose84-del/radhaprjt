namespace FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCost
{
    public class AssetAdditionalCostDto
    {
        public int Id { get; set; }
        public int AssetId { get; set; }   
        public int AssetSourceId { get; set; }   
        public decimal Amount { get; set; }
        public string? JournalNo { get; set; }
        public int? CostType { get; set; }    
    }
}