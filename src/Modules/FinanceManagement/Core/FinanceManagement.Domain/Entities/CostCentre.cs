using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    /// <summary>
    /// Cost Centre master &amp; 3-level hierarchy (US-GL05-01).
    /// A self-referencing tree, unit-wise (UnitId / CompanyId resolved from the JWT, never the payload):
    ///   L1 Plant            → tied to the JWT Unit; ParentCostCentreId = null
    ///   L2 Department Group → links UserManagement.DepartmentGroup; parent = an L1 Plant cost centre
    ///   L3 Department       → links UserManagement.Department;      parent = an L2 Department-Group cost centre
    /// UnitId (the "Plant") is inherited down the tree from the parent and is consistent with the JWT unit.
    /// CostCentreCode is unique per unit (the same code is allowed in a different unit) and immutable after create.
    /// ResponsibleManagerId / EffectiveFromDate / EffectiveToDate are reserved columns (saved null for now).
    /// CC-mandatory journal tagging, payroll split, FA depreciation auto-tag and rollup reports are separate stories.
    /// </summary>
    public class CostCentre : BaseEntity
    {
        // Unit-wise scope — both sourced from the JWT (GetUnitId / GetCompanyId), not the request body.
        public int UnitId { get; set; }        // the "Plant"
        public int CompanyId { get; set; }

        public string? CostCentreCode { get; set; }   // unique per unit, immutable
        public string? CostCentreName { get; set; }

        // Level dropdown -> Finance.MiscMaster (same-module FK): L1 Plant / L2 Department Group / L3 Department.
        // Ordinal ("parent one level up") is read from the stable MiscMaster.SortOrder/Code, never from this Id.
        public int CentreLevelId { get; set; }
        public MiscMaster? CentreLevelMaster { get; set; }

        // Same-module self-reference (the parent one level up). Null only for L1.
        public int? ParentCostCentreId { get; set; }
        public CostCentre? ParentCostCentre { get; set; }
        public ICollection<CostCentre>? ChildCostCentres { get; set; }

        // Cross-module links (UserManagement) — no DB FK constraint (lookup pattern).
        public int? DepartmentGroupId { get; set; }    // set for L2 &amp; L3
        public int? DepartmentId { get; set; }         // set for L3 only

        // Reserved — populated by a later story (manager-change alert routing / effective dating).
        public int? ResponsibleManagerId { get; set; }
        public DateTimeOffset? EffectiveFromDate { get; set; }
        public DateTimeOffset? EffectiveToDate { get; set; }
    }
}
