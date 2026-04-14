namespace SalesManagement.Application.SalesOrder.Queries.GetAgentCommissions
{
    public sealed class AgentCommissionsDto
    {
        public int Id { get; set; }

        public int AgentId { get; set; }
        public string? AgentName { get; set; }

        public int TriggerEventId { get; set; }
        public string? TriggerEventName { get; set; }

        public int CommissionTypeId { get; set; }
        public string? CommissionTypeName { get; set; }

        public int CommissionBasisId { get; set; }
        public string? CommissionBasisName { get; set; }

        public decimal CommissionPercentage { get; set; }

        public int? SlabTypeId { get; set; }
        public string? SlabTypeName { get; set; }

        public DateTimeOffset ValidityFrom { get; set; }
        public DateTimeOffset? ValidityTo { get; set; }

        public List<AgentCommissionSalesGroupInfoDto> AgentCommissionSalesGroups { get; set; } = [];
        public List<AgentCommissionPaymentTermInfoDto> AgentCommissionPaymentTerms { get; set; } = [];
        public List<AgentCommissionSlabInfoDto> AgentCommissionSlabs { get; set; } = [];
    }

    public sealed class AgentCommissionSalesGroupInfoDto
    {
        public int Id { get; set; }
        public int SalesGroupId { get; set; }
        public string? SalesGroupName { get; set; }
    }

    public sealed class AgentCommissionPaymentTermInfoDto
    {
        public int Id { get; set; }
        public int PaymentTermId { get; set; }
        public string? PaymentTermDescription { get; set; }
    }

    public sealed class AgentCommissionSlabInfoDto
    {
        public int Id { get; set; }
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
