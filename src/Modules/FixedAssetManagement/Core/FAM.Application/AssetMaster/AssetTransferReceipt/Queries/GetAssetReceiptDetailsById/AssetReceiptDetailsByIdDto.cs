namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById
{
    public class AssetReceiptDetailsByIdDto
    {
        public int AssetReceiptId {get;set;}
        public int AssetTransferId {get;set;}
        public int AssetId {get;set;}
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; } 
        public string? LocationName { get; set; } 
        public string? SubLocationName { get; set; } 
        public string? UserID { get; set; } 
        public string? UserName { get; set; } 


    }
}