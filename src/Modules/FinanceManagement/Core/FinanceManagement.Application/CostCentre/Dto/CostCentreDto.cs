namespace FinanceManagement.Application.CostCentre.Dto
{
    public class CostCentreDto
    {
        public int Id { get; set; }

        public int UnitId { get; set; }
        public string? UnitName { get; set; }          // populated via IUnitLookup (the "Plant")
        public int CompanyId { get; set; }

        public string? CostCentreCode { get; set; }
        public string? CostCentreName { get; set; }

        public int CentreLevelId { get; set; }
        public string? CentreLevelName { get; set; }   // Finance.MiscMaster.Description (same-module JOIN)

        public int? ParentCostCentreId { get; set; }
        public string? ParentCostCentreName { get; set; } // self-JOIN

        public int? DepartmentGroupId { get; set; }
        public string? DepartmentGroupName { get; set; }  // populated via IDepartmentGroupLookup

        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }       // populated via IDepartmentLookup

        public int? ResponsibleManagerId { get; set; }
        public string? ResponsibleManagerName { get; set; }
        public DateTimeOffset? EffectiveFromDate { get; set; }
        public DateTimeOffset? EffectiveToDate { get; set; }

        // Open-transaction count for the current period — stub (0) until the journal engine lands (AC#3).
        public int OpenTxns { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
