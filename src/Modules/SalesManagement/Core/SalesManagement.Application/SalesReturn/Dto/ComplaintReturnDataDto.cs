namespace SalesManagement.Application.SalesReturn.Dto
{
    public class ComplaintReturnDataDto
    {
        public int ComplaintHeaderId { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly ComplaintDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ResolutionType { get; set; }
        public List<ComplaintInvoiceItemDto>? InvoiceItems { get; set; }
        public List<BagStatusLookupDto>? BagStatuses { get; set; }
    }

    public class BagStatusLookupDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }

    public class ComplaintInvoiceItemDto
    {
        public int InvoiceHeaderId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int InvoiceDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? LotId { get; set; }
        public string? LotCode { get; set; }
        public int NumberOfPacks { get; set; }
        public decimal NetWeight { get; set; }
        public decimal InvoiceAmount { get; set; }
        public int? PackTypeId { get; set; }

        // Dispatch pack range for validation
        public int DispatchStartPackNo { get; set; }
        public int DispatchEndPackNo { get; set; }

        // Already returned packs (for overlap validation)
        public List<PackRangeDto>? ReturnedPackRanges { get; set; }
    }

    public class PackRangeDto
    {
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
    }
}
