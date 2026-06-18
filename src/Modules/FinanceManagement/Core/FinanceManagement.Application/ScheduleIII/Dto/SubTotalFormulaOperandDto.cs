namespace FinanceManagement.Application.ScheduleIII.Dto
{
    // A candidate operand for the "Edit formula" dialog — a P&L line item or a sub-total node.
    public class SubTotalFormulaOperandDto
    {
        public int Id { get; set; }            // line item id (Kind=LINEITEM) or MiscMaster S3_SUBTOTAL_TYPE id (Kind=SUBTOTAL)
        public string? Name { get; set; }      // display label (e.g. "Revenue from Operations", "EBITDA")
        public string? Kind { get; set; }      // "LINEITEM" | "SUBTOTAL"
    }
}
