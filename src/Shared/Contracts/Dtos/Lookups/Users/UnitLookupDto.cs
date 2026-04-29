namespace Contracts.Dtos.Lookups.Users
{
    public class UnitLookupDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public string ShortName { get; set; } = default!;
        public string UnitHeadName { get; set; } = default!;
        public string OldUnitId { get; set; } = default!;
        public int? SpindlesCapacity { get; set; }
        public int UnitTypeId { get; set; }
        public string? UnitTypeName { get; set; }
        public int DivisionId { get; set; }
        // Plant/Unit → Company bridge — needed by cross-module callers that traverse
        // Unit → Company → GSTIN (e.g., e-waybill generation).
        public int CompanyId { get; set; }
        // Sourced from AppData.UnitAddress (one address per unit). Nullable when no address row exists.
        public string? PinCode { get; set; }

    }
}