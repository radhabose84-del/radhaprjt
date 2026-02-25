namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue
{
    public class UpdateAssetTransferDtlDto
    {
       public int AssetTransferId { get; set; }
        public int AssetId { get; set; }
       public decimal AssetValue { get; set; } 
    }
}