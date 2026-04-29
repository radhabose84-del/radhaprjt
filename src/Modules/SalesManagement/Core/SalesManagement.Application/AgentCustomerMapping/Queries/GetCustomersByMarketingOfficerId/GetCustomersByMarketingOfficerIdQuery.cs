using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetCustomersByMarketingOfficerId
{
    public class GetCustomersByMarketingOfficerIdQuery : IRequest<ApiResponseDTO<List<AgentCustomerMappingDto>>>
    {
        public int MarketingOfficerId { get; set; }
    }
}
