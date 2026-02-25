namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UpdateAssetMasterGeneral
{
    public class AssetAdditionalCostUpdateDto
    {
        public int Id { get; set; }
        public int AssetId { get; set; } 
        public int AssetSourceId { get; set; }   
        public decimal Amount { get; set; }
        public string? JournalNo { get; set; }
        public int? CostType { get; set; }   
    }
}