namespace SalesManagement.Application.AgentPortal.Dto
{
    public class AgentEnquiryListDto
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public DateTimeOffset EnquiryDate { get; set; }
        public string? ContactPerson { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    }
}
