namespace SalesManagement.Application.ProformaInvoice.Dto
{
    public class ProformaInvoiceDto
    {
        public int Id { get; set; }
        public string? ProformaNumber { get; set; }
        public DateOnly ProformaDate { get; set; }
        public int SalesOrderId { get; set; }
        public string? SalesOrderNo { get; set; }
        public int PartyId { get; set; }
        public string? CustomerName { get; set; }
        public decimal ProformaAmount { get; set; }
        public decimal SOBalance { get; set; }
        public decimal PaymentReceivedAmount { get; set; }
        public bool PaymentReceivedFlag { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
