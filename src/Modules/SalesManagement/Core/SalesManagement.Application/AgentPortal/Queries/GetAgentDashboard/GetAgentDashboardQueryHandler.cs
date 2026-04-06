using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.AgentPortal.Dto;
using SalesManagement.Application.Common.Interfaces.IAgentPortal;

namespace SalesManagement.Application.AgentPortal.Queries.GetAgentDashboard
{
    public class GetAgentDashboardQueryHandler : IRequestHandler<GetAgentDashboardQuery, ApiResponseDTO<AgentDashboardDto>>
    {
        private readonly IAgentPortalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetAgentDashboardQueryHandler(IAgentPortalQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<AgentDashboardDto>> Handle(GetAgentDashboardQuery request, CancellationToken cancellationToken)
        {
            var partyId = _ipAddressService.GetPartyId();
            if (!partyId.HasValue)
                return new ApiResponseDTO<AgentDashboardDto> { IsSuccess = false, Message = "Agent not identified." };

            var customerIds = await _queryRepository.GetAgentCustomerIdsAsync(partyId.Value);
            var data = await _queryRepository.GetDashboardAsync(partyId.Value, customerIds);

            return new ApiResponseDTO<AgentDashboardDto> { IsSuccess = true, Message = "Success", Data = data };
        }
    }
}
