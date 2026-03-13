using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.AgentCustomerMapping.Commands.CreateAgentCustomerMapping
{
    public class CreateAgentCustomerMappingCommand : IRequest<ApiResponseDTO<int>>
    {
        public int CustomerId { get; set; }
        public int AgentId { get; set; }
        public int? SubAgentId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsDefaultAgent { get; set; }
        public string? Remarks { get; set; }
    }
}
