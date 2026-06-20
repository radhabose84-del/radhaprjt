using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    /// <summary>
    /// Profit Centre master &amp; 2-level hierarchy (US-GL05-02).
    /// A self-referencing tree (max 2 levels):
    ///   L1 Segment      → a revenue segment (e.g. Spinning, Weaving); ParentProfitCentreId = null
    ///   L2 Sub-segment  → rolls up to an L1 Segment; parent = an L1 Segment profit centre
    /// CompanyId is the owning company (resolved from the JWT, never the payload). It is stored for audit
    /// only — ProfitCentreCode is unique ACROSS all companies (group segment reporting), so uniqueness is
    /// NOT scoped by company. ProfitCentreCode is immutable after create.
    /// The "Segment (inherited)" shown in the UI is display-only (= parent name for L2); it is not stored.
    /// ResponsibleHeadId is reserved/optional (saved null until the FE wires the picker), resolved via IUserLookup.
    /// FY-transaction count and the year-end deactivation guard are stubbed (0 / false) until the GL journal
    /// engine tags transactions to profit centres. PC-mandatory journal tagging and PC P&amp;L are separate stories.
    /// </summary>
    public class ProfitCentre : BaseEntity
    {
        // Owning company — sourced from the JWT (GetCompanyId), not the request body.
        // Stored for audit only; code uniqueness is global (across companies), so this is NOT a uniqueness key.
        public int CompanyId { get; set; }

        public string? ProfitCentreCode { get; set; }   // unique across all companies, immutable
        public string? ProfitCentreName { get; set; }

        // Level dropdown -> Finance.MiscMaster (same-module FK): L1 Segment / L2 Sub-segment.
        // Ordinal ("parent one level up") is read from the stable MiscMaster.SortOrder/Code, never from this Id.
        public int LevelId { get; set; }
        public MiscMaster? LevelMaster { get; set; }

        // Same-module self-reference (the parent one level up). Null only for L1 Segment.
        public int? ParentProfitCentreId { get; set; }
        public ProfitCentre? ParentProfitCentre { get; set; }
        public ICollection<ProfitCentre>? ChildProfitCentres { get; set; }

        // Cross-module link (UserManagement) — no DB FK constraint (lookup pattern). Optional/reserved.
        public int? ResponsibleHeadId { get; set; }

        // "Revenue Linked" flag — segment PCs are revenue-linked by default.
        public bool IsRevenueLinked { get; set; } = true;

        // AC#4 — justification captured when a PC is added mid-year. Prior transactions cannot be
        // retro-tagged; that note is written to the audit log on create.
        public string? MidYearJustification { get; set; }
    }
}
