namespace SalesManagement.Application.Complaint.Dto
{
    public class CreateComplaintDetailDto
    {
        public int InvoiceHeaderId { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int InvoiceTypeId { get; set; }
        public int? LotId { get; set; }
        public int ItemId { get; set; }
        public int NumberOfPacks { get; set; }
        public decimal NetWeight { get; set; }
        public decimal InvoiceAmount { get; set; }
        public int? DivisionId { get; set; }
        public List<int>? NatureOfComplaintIds { get; set; }
    }
}
