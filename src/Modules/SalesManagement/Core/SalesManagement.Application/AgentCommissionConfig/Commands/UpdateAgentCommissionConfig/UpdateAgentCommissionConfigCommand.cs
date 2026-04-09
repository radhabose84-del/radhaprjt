using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig;

namespace SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig
{
    public class UpdateAgentCommissionConfigCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public int CommissionTypeId { get; set; }
        public int CommissionBasisId { get; set; }
        public int ApplicableLevelId { get; set; }
        public decimal CommissionPercentage { get; set; }
        public DateTimeOffset ValidityFrom { get; set; }
        public DateTimeOffset? ValidityTo { get; set; }
        public int TriggerEventId { get; set; }
        public int? SlabTypeId { get; set; }
        public int CommissionSplitId { get; set; }
        public int IsActive { get; set; }
        public List<int>? SalesGroupIds { get; set; }
        public List<int>? PaymentTermIds { get; set; }
        public List<AgentCommissionSlabItem>? Slabs { get; set; }
    }
}
