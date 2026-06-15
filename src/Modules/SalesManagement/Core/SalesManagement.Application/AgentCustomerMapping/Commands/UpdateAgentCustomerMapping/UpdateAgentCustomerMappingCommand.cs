using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.AgentCustomerMapping.Commands.UpdateAgentCustomerMapping
{
    public class UpdateAgentCustomerMappingCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int AgentId { get; set; }
        public int? SubAgentId { get; set; }
        public int SalesGroupId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsDefaultAgent { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
