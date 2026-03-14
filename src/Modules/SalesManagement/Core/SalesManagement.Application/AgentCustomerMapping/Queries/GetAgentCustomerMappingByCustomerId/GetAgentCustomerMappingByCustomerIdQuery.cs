using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingByCustomerId
{
    public class GetAgentCustomerMappingByCustomerIdQuery : IRequest<ApiResponseDTO<List<AgentCustomerMappingDto>>>
    {
        public int CustomerId { get; set; }
    }
}
