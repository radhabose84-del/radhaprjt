namespace SalesManagement.Application.SalesOrder.Dto
{
    /// <summary>
    /// Intermediate row shapes for the GetAgentCommissions query.
    /// Used by the Dapper query in SalesOrderQueryRepository; not exposed as response DTOs.
    /// </summary>
    public sealed class AgentCommissionSalesGroupRow
    {
        public int Id { get; set; }
        public int AgentCommissionConfigId { get; set; }
        public int SalesGroupId { get; set; }
        public string? SalesGroupName { get; set; }
    }

    public sealed class AgentCommissionPaymentTermRow
    {
        public int Id { get; set; }
        public int AgentCommissionConfigId { get; set; }
        public int PaymentTermId { get; set; }
    }

    public sealed class AgentCommissionSlabRow
    {
        public int Id { get; set; }
        public int AgentCommissionConfigId { get; set; }
        public int SlabOrder { get; set; }
        public int FromDelay { get; set; }
        public int? ToDelay { get; set; }
        public int CommissionTypeId { get; set; }
        public string? CommissionTypeName { get; set; }
        public int CommissionBasisId { get; set; }
        public string? CommissionBasisName { get; set; }
        public decimal CommissionValue { get; set; }
    }
}
