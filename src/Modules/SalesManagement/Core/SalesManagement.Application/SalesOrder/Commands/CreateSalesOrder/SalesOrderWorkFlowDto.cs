namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder
{
    public class SalesOrderWorkFlowDto
    {
        public int Id { get; set; }
        public string? SalesOrderNo { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? UnitId { get; set; }

        // Drives the conditional MD-Discount approval step.
        // sp_EvaluateApproval reads $.Header.IsMdDiscountEnabled when evaluating
        // ApprovalRuleCondition for the MD step. Header-only — line rows leave
        // this default (false) since line-level conditions don't apply.
        public bool IsMdDiscountEnabled { get; set; }
    }
}
