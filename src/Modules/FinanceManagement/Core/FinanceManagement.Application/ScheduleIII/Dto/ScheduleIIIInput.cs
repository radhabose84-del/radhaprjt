namespace FinanceManagement.Application.ScheduleIII.Dto
{
    // Composite Create/Update payload — one call inserts/updates Structure + Sections + LineItems
    // + SubTotals + Formulas (all five tables).
    public class ScheduleIIIInput
    {
        public int Id { get; set; }                 // structure id (Update only)
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
        public int StructureStatusId { get; set; }
        public int TextileSplitEnabled { get; set; } // 0/1
        public int VersionNo { get; set; } = 1;
        public int IsActive { get; set; } = 1;       // Update only

        public List<SectionInput> Sections { get; set; } = new();
        public List<SubTotalInput> SubTotals { get; set; } = new();
    }

    public class SectionInput
    {
        public string? SectionName { get; set; }
        public int StatementTypeId { get; set; }
        public int NatureId { get; set; }
        public int DisplayOrder { get; set; }
        public List<LineItemInput> LineItems { get; set; } = new();
    }

    public class LineItemInput
    {
        public string? LineCode { get; set; }
        public string? LineName { get; set; }
        public string? ParentLineCode { get; set; }   // resolved to ParentLineId after insert (within the same structure)
        public string? SubClassification { get; set; }
        public string? NoteReference { get; set; }
        public int DisplayOrder { get; set; }
        public int IsSplitLine { get; set; }          // 0/1
    }

    public class SubTotalInput
    {
        public string? SubTotalName { get; set; }
        public int IncludeOtherIncome { get; set; }   // 0/1
        public int DisplayOrder { get; set; }
        public List<FormulaInput> Formulas { get; set; } = new();
    }

    public class FormulaInput
    {
        public int OperandTypeId { get; set; }        // MiscMaster (S3_OPERAND_TYPE): LineItem / SubTotal
        public string? OperandCode { get; set; }      // LineCode (line operand) or SubTotalName (sub-total operand) — resolved to OperandRefId
        public int OperatorId { get; set; }           // MiscMaster (S3_OPERATOR): Plus / Minus
        public int DisplayOrder { get; set; }
    }
}
