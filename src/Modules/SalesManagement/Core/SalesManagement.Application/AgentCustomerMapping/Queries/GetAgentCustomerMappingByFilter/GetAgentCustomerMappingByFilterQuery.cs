using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingByFilter
{
    public class GetAgentCustomerMappingByFilterQuery : IRequest<ApiResponseDTO<List<AgentCustomerMappingDto>>>
    {
        public int? SalesGroupId { get; set; }
        public int? CustomerId { get; set; }
    }
}
