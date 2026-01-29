
namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral
{
    public class AssetAdditionalCostCombineDto
    {
        public int AssetSourceId { get; set; }   
        public decimal Amount { get; set; }
        public string? JournalNo { get; set; }
        public int? CostType { get; set; }   
    }
}