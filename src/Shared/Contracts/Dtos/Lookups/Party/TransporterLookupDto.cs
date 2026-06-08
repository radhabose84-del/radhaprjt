namespace Contracts.Dtos.Lookups.Party
{
    /// <summary>
    /// Lightweight transporter projection from the ERP Party Master.
    /// Used by the Freight RFQ transporter dropdown / comparison rows.
    /// </summary>
    public class TransporterLookupDto
    {
        public int Id { get; set; }
        public string TransporterCode { get; set; } = string.Empty;
        public string TransporterName { get; set; } = string.Empty;
    }
}
