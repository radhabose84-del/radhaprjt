namespace GateEntryManagement.Domain.Common
{
    public static class MiscEnumEntity
    {
        public const string GateEntryImage = "GateEntryImage";

        // Server base URL for file preview (per-environment: 126 dev / 130 QA)
        public const string ImagePath = "ImagePath";

        public const string Approved = "Approved";

        // VMR Status
        public const string VMRStatus = "VMRStatus";
        public const string VMRStatusInsidePremises = "IN";
        public const string VMRStatusExited = "OUT";

        // GatePass
        public const string TransactionTypeGatePass = "Gate Outward";

        // GateInward
        public const string TransactionTypeGateInward = "Gate Inward";

        // Document Sequence
        public const string TransactionTypeGateEntry = "Gate Entry";
        public const string ModuleGateEntry = "Sales";
    }
}
