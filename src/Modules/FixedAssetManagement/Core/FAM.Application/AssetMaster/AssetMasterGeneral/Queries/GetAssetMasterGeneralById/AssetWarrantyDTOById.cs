namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById
{
    public class AssetWarrantyDTOById
    {
        public int Id { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int Period { get; set; }
        public string? WarrantyType { get; set; }
        public string? ServiceClaimStatus { get; set; }
        public string? WarrantyProvider { get; set; }
        public string? MobileNumber { get; set; }
        public string? ContactPerson { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public string? Document { get; set; }
        public string? CountryName { get; set; }
        public string? StateName { get; set; }
        public string? CityName { get; set; }
        public string? ServicePinCode { get; set; }
        public string? ServiceAddressLine1 { get; set; }
        public string? ServiceAddressLine2 { get; set; }
        public string? ServiceContactPerson { get; set; }
        public string? ServiceMobileNumber { get; set; }
        public string? ServiceEmail { get; set; }
        public string? ServiceClaimProcessDescription { get; set; }
        public string? ServiceLastClaimDate { get; set; }
        public int WarrantyTypeId { get; set; }
        public int ServiceClaimStatusId { get; set; }
        public int ServiceCountryId { get; set; }
        public int ServiceStateId { get; set; }
        public int ServiceCityId { get; set; }
    }
}