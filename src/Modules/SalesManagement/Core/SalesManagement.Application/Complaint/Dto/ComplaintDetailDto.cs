namespace SalesManagement.Application.Complaint.Dto
{
    public class ComplaintDetailDto
    {
        public int Id { get; set; }
        public int ComplaintHeaderId { get; set; }
        public int InvoiceHeaderId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int InvoiceTypeId { get; set; }
        public string? InvoiceTypeName { get; set; }
        public int? LotId { get; set; }
        public string? LotCode { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int NumberOfPacks { get; set; }
        public decimal NetWeight { get; set; }
        public decimal InvoiceAmount { get; set; }
        public int? DivisionId { get; set; }
        public string? DivisionName { get; set; }

        // Nature of Complaint IDs (multi-select)
        public List<ComplaintDetailNatureDto>? NatureOfComplaints { get; set; }
    }
}
