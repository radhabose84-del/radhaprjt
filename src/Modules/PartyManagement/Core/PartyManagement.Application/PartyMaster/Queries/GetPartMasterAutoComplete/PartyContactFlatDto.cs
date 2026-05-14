namespace PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete
{
    public class PartyContactFlatDto
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Designation { get; set; }
        public string? EmailID { get; set; }
        public string? AlternateEmailId { get; set; }
        public string? MobileNo { get; set; }
        public string? AlternateMobileNumber { get; set; }
        public string? Phone { get; set; }
        public int? GenderId { get; set; }
        public string? GenderName { get; set; }
        public int? PreferredChannelId { get; set; }
        public string? PreferredChannelName { get; set; }
        public int? ContactTypeId { get; set; }
        public string? ContactTypeName { get; set; }
        public string? ContactBy { get; set; }
    }
}
