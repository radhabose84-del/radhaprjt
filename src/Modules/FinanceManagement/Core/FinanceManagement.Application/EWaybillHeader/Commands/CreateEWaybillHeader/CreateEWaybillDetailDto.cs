namespace FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader
{
    /// <summary>
    /// Line item payload for CreateEWaybillHeaderCommand.
    /// Each entry becomes one Finance.EWaybillDetail row, inserted in the same save as the header.
    /// </summary>
    public class CreateEWaybillDetailDto
    {
        public int ItemSno { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? HsnNo { get; set; }
        public string? IsService { get; set; } = "N";   // goods by default
        public decimal Qty { get; set; }
        public string? UOM { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal TaxRate { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }
    }
}
