namespace PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete
{
    public class SalesTypeFlatDto
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public int? SalesSegmentId { get; set; }
        public int? PaymentTermsId { get; set; }
        public int? SalesGroupId { get; set; }
        public int? SalesOfficeId { get; set; }
    }
}
