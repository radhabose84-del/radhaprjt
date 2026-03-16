namespace FinanceManagement.Application.EInvoiceHeader.Dto
{
    /// <summary>
    /// Transport details for e-Waybill generation.
    /// Used both in combined IRN+EWB (Case 1) and standalone EWB (Case 2).
    /// </summary>
    public class EwbTransportDetails
    {
        public string? TransId { get; set; }
        public string? TransName { get; set; }
        public string? TransMode { get; set; }
        public int Distance { get; set; }
        public string? TransDocNo { get; set; }
        public string? TransDocDt { get; set; }
        public string? VehNo { get; set; }
        public string? VehType { get; set; }
    }
}
