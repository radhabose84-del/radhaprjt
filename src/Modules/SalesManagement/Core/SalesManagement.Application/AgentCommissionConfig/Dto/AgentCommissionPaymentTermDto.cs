namespace SalesManagement.Application.AgentCommissionConfig.Dto
{
    public class AgentCommissionPaymentTermDto
    {
        public int Id { get; set; }
        public int PaymentTermId { get; set; }
        public string? PaymentTermCode { get; set; }
        public string? PaymentTermDescription { get; set; }
    }
}
