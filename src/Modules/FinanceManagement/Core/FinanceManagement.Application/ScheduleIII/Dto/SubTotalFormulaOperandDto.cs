namespace FinanceManagement.Application.ScheduleIII.Dto
{
    // A candidate operand for the "Edit formula" dialog — a P&L line item or a sub-total node.
    // When a subTotalId is supplied, the row also reflects how that sub-total currently uses the operand
    // (off / + / −) via a LEFT JOIN to ScheduleIIISubTotalFormula.
    public class SubTotalFormulaOperandDto
    {
        public int Id { get; set; }            // line item id (Kind=LINEITEM) or MiscMaster S3_SUBTOTAL_TYPE id (Kind=SUBTOTAL)
        public string? Name { get; set; }      // display label (e.g. "Revenue from Operations", "EBITDA")
        public string? Kind { get; set; }      // "LINEITEM" | "SUBTOTAL"

        // Current usage in the sub-total being edited (null/false when not used or no subTotalId given).
        public bool IsSelected { get; set; }
        public int? OperatorId { get; set; }     // MiscMaster S3_OPERATOR id (PLUS / MINUS) when selected
        public string? OperatorCode { get; set; } // "PLUS" | "MINUS" | null  → maps to + / − / off
        public int? DisplayOrder { get; set; }
    }
}
