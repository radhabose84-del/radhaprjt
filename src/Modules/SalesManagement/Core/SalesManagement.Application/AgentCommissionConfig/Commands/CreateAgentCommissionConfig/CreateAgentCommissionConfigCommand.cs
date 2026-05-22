using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig
{
    public class CreateAgentCommissionConfigCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
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
        public List<int>? SalesGroupIds { get; set; }
        public List<int>? PaymentTermIds { get; set; }
        public List<AgentCommissionSlabItem>? Slabs { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }

    public class AgentCommissionSlabItem
    {
        public int SlabOrder { get; set; }
        public int FromDelay { get; set; }
        public int? ToDelay { get; set; }
        public int CommissionTypeId { get; set; }
        public int CommissionBasisId { get; set; }
        public decimal CommissionValue { get; set; }
    }
}
