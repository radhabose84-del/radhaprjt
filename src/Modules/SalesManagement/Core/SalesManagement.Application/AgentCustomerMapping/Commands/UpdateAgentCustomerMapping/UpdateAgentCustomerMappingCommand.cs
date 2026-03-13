using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.AgentCustomerMapping.Commands.UpdateAgentCustomerMapping
{
    public class UpdateAgentCustomerMappingCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public int? SubAgentId { get; set; }
        public int SalesSegmentId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsDefaultAgent { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
    }
}
