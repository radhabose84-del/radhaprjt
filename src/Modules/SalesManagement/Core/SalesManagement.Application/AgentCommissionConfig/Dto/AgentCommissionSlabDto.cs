namespace SalesManagement.Application.AgentCommissionConfig.Dto
{
    public class AgentCommissionSlabDto
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
