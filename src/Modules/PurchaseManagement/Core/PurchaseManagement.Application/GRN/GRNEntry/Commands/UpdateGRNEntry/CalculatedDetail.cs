namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry
{
    public class CalculatedDetail
    {
        public int Id { get; set; }   // Must match DB detail Id
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal ItemValue { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal DiscountValue { get; set; }
        public string? GrnDetailImage { get; set; }
    }
}