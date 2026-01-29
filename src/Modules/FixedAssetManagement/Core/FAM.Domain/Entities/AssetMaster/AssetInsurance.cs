using FAM.Domain.Common;

namespace FAM.Domain.Entities.AssetMaster
{
    public class AssetInsurance  : BaseEntity
    {
        public new int Id { get; set; }
        public int  AssetId { get; set; }
         public AssetMasterGenerals? AssetMaster  { get; set; }         
        public string? PolicyNo { get; set; }       
        public DateOnly StartDate { get; set; } 
        public int Insuranceperiod { get; set; }       
        public DateOnly EndDate { get; set; }
        public decimal? PolicyAmount { get; set; }
        public string? VendorCode { get; set; }
        public int RenewalStatus { get; set; }
        public DateOnly RenewedDate { get; set; }
             
    
    }
}