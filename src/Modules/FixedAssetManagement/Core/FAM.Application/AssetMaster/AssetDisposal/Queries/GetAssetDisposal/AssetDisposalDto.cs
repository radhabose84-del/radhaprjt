namespace FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal
{
    public class AssetDisposalDto
    {
        public int Id { get; set; }
        public int AssetId { get; set; } 
        public int AssetPurchaseId { get; set; } 
        public DateOnly DisposalDate { get; set; }
        public int? DisposalType { get; set; }  
        public string? DisposalReason { get; set; }
        public decimal? DisposalAmount { get; set; }
    }
}