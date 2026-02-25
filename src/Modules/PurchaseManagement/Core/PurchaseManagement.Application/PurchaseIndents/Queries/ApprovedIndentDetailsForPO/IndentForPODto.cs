namespace PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO
{
    public class IndentForPODto
    {
        public int Id { get; set; }
        public string IndentNumber { get; set; } = default!;
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public string IndentTypeName { get; set; } = default!;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public string Purpose { get; set; } = default!;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = default!;
        public int? RfqId { get; set; }
        public int? QuotationId { get; set; }
        public int? HasPriceMaster { get; set; }  
        public string? PendingReason { get; set; }
        
        public ICollection<IndentDetailsForPODto> IndentDetails { get; set; } = default!;
        public ICollection<IndentDutyForPODto> IndentDutyDetails { get; set; } = default!;
    }
}