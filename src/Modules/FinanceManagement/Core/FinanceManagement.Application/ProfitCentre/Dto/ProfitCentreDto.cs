namespace FinanceManagement.Application.ProfitCentre.Dto
{
    public class ProfitCentreDto
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }       // populated via ICompanyLookup

        public string? ProfitCentreCode { get; set; }
        public string? ProfitCentreName { get; set; }

        public int LevelId { get; set; }
        public string? LevelName { get; set; }          // Finance.MiscMaster.Description (same-module JOIN)

        public int? ParentProfitCentreId { get; set; }
        public string? ParentProfitCentreName { get; set; } // self-JOIN; "Segment (inherited)" in the UI

        public int? ResponsibleHeadId { get; set; }
        public string? ResponsibleHeadName { get; set; }    // populated via IUserLookup

        public bool IsRevenueLinked { get; set; }

        public string? MidYearJustification { get; set; }

        // Current-FY transaction count — stub (0) until the GL journal engine lands (AC#5).
        public int FyTransactionCount { get; set; }

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
