namespace SalesManagement.Application.Common.Constants
{
    /// <summary>
    /// String-code constants for MiscType / MiscMaster rows referenced by Sales-module entities.
    /// These match the seed-script values inserted into <c>Sales.MiscTypeMaster</c> and <c>Sales.MiscMaster</c>.
    /// </summary>
    public static class MiscMasterCodes
    {
        // ── SalesOrderTypeMaster.SoTypeId ────────────────────────────────────
        public const string SOTM_TYPE_MISCTYPE = "SOTM_TYPE";

        public const string SO_NORMAL   = "SO_NORMAL";
        public const string SO_RATE_AGR = "SO_RATE_AGR";
        public const string SO_SAMPLE   = "SO_SAMPLE";

        // ── SalesEnquiryHeader.EnquiryTypeId ─────────────────────────────────
        public const string ENQ_TYPE_MISCTYPE = "ENQ_TYPE";

        public const string ENQ_DOMESTIC = "ENQ_DOMESTIC";
        public const string ENQ_EXPORT   = "ENQ_EXPORT";
        public const string ENQ_SAMPLE   = "ENQ_SAMPLE";
        public const string ENQ_REPEAT   = "ENQ_REPEAT";
    }
}
