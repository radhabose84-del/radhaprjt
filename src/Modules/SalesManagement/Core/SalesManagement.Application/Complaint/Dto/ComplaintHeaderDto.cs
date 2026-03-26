namespace SalesManagement.Application.Complaint.Dto
{
    public class ComplaintHeaderDto
    {
        public int Id { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly ComplaintDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }

        // Customer Snapshot
        public string? CustomerAddress { get; set; }
        public string? CustomerPIN { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPAN { get; set; }
        public string? CustomerGSTNo { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal TotalOS { get; set; }
        public decimal Outstanding { get; set; }
        public decimal BalanceCredit { get; set; }
        public string? Delay { get; set; }
        public string? Ledger { get; set; }

        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
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

        // Details (populated in GetById)
        public List<ComplaintDetailDto>? ComplaintDetails { get; set; }
    }
}
