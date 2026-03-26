namespace SalesManagement.Application.Complaint.Dto
{
    public class InvoiceSearchDto
    {
        public int Sno { get; set; }
        public int InvoiceDetailId { get; set; }
        public int InvoiceHeaderId { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int InvoiceTypeId { get; set; }
        public string? InvoiceTypeName { get; set; }
        public string? InvoiceNo { get; set; }
        public string? ShortCode { get; set; }         // InvoiceType MiscMaster Code
        public int ItemId { get; set; }
        public string? ProductCode { get; set; }        // Item Code
        public string? ProductName { get; set; }        // Item Name
        public int? LotId { get; set; }
        public string? LotNum { get; set; }             // Lot Code
        public int UnitId { get; set; }            // Internal — used for Division resolution
        public int? DivisionId { get; set; }
        public string? DivisionCode { get; set; }       // Division ShortName
        public int Packs { get; set; }                  // NoOfBags
        public decimal NetWeight { get; set; }          // Quantity
        public decimal InvAmount { get; set; }          // TotalAmount
        public int? UOMId { get; set; }
        public string? UOMName { get; set; }
    }
}
