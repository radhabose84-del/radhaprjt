namespace Contracts.Dtos.Lookups.QC
{
    /// <summary>
    /// Lightweight QC.MiscMaster lookup row (e.g. QC status — Pending / Approved / Rejected).
    /// Used by cross-module consumers (Purchase Arrival/GRN) to resolve a QcStatusId to its
    /// code/name without a cross-module JOIN.
    /// </summary>
    public sealed class QcMiscMasterLookupDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
}
