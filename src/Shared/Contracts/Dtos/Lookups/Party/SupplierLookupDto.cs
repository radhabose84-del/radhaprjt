namespace Contracts.Dtos.Lookups.Party
{
    /// <summary>
    /// Lightweight supplier (vendor) projection from the ERP Party Master.
    /// Used by the External Service Request vendor dropdown.
    /// </summary>
    public class SupplierLookupDto
    {
        public int Id { get; set; }
        public string VendorCode { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
    }
}
