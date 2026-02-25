namespace FAM.Domain.Entities.AssetMaster
{
    public class AssetTransferIssueDtl
    {
        public int Id {get;set;}
        public int AssetTransferId { get; set; }        
        public AssetTransferIssueHdr AssetTransferIssueHdr { get; set; } = null!; 
        public int AssetId { get; set; } 
        public AssetMasterGenerals AssetMasterTransferIssue  { get; set; } = null!; 
        public decimal AssetValue {get;set;}
      

    }
}