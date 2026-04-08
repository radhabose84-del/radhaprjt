namespace SalesManagement.Application.DispatchAddressMapping.Dto
{
    public class DispatchAddressMappingDto
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public int DispatchAddressId { get; set; }
        public string? DispatchAddressName { get; set; }
        public string? DispatchAddressLine1 { get; set; }
        public int UsageTypeId { get; set; }
        public string? UsageTypeName { get; set; }
        public bool IsDefault { get; set; }
        public int FreightId { get; set; }
        public string? FreightModeName { get; set; }
        public string? RateMethodName { get; set; }
        public decimal? FreightRate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
