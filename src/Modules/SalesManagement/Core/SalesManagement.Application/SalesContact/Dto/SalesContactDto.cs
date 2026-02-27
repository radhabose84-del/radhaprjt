namespace SalesManagement.Application.SalesContact.Dto
{
    public class SalesContactDto
    {
        public int Id { get; set; }
        public string? ContactName { get; set; }
        public string? MobileNumber { get; set; }
        public int ContactTypeId { get; set; }
        public string? ContactTypeName { get; set; }   // from SQL JOIN (MiscMaster.Description)
        public int? PartyId { get; set; }
        public string? PartyName { get; set; }         // from IPartyLookup (cross-module)
        public string? Email { get; set; }
        public string? Remarks { get; set; }
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
