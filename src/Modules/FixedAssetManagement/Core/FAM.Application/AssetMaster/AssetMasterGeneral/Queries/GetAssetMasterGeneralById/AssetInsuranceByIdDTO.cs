namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById
{
    public class AssetInsuranceByIdDTO
    {
        public int Id { get; set; }
        public string? PolicyNo { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int Insuranceperiod { get; set; }
        public string? PolicyAmount { get; set; }
        public string? VendorCode { get; set; }
        public string? RenewalStatus { get; set; }
        public string? RenewedDate { get; set; }
        public byte IsActive { get; set; }
        
    }
}