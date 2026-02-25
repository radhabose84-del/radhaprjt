using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance
{
    public class GetAssetInsuranceDto
    {
        public int Id { get; set; }
        public int  AssetId { get; set; }       
        public string? PolicyNo { get; set; }       
        public DateOnly StartDate { get; set; }
        public string? Insuranceperiod { get; set; }
        public DateOnly EndDate { get; set; }
        public string? PolicyAmount { get; set; }
        public string? VendorCode { get; set; }
        public int RenewalStatus { get; set; }
        public DateOnly RenewedDate { get; set; }
        public Status IsActive { get; set; }

        
    }
}