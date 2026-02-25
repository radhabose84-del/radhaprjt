namespace FAM.Domain.Entities.AssetMaster
{
    public class AssetTransferReceiptDtl
    {
        public int Id {get;set;}
        public int AssetReceiptId { get; set; }        
        public AssetTransferReceiptHdr AssetTransferReceiptHdr { get; set; } = null!; 
        public int AssetId { get; set; } 
        public AssetMasterGenerals AssetMasterTransferReceipt  { get; set; } = null!; 
        public int? LocationId { get; set; }
        public Location? Location { get; set; }
        public int? SubLocationId { get; set; } 
        public SubLocation? SubLocation { get; set; }
        public string? UserID { get; set; }
        public string? UserName { get; set; }   
        public byte AckStatus { get; set; }
        public DateTimeOffset? AckDate { get; set; }


    }
}