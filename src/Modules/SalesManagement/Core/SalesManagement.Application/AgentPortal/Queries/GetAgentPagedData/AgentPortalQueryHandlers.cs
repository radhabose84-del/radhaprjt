using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.AgentPortal.Dto;
using SalesManagement.Application.Common.Interfaces.IAgentPortal;

namespace SalesManagement.Application.AgentPortal.Queries.GetAgentPagedData
{
    public class GetAgentMyCustomersQueryHandler : IRequestHandler<GetAgentMyCustomersQuery, ApiResponseDTO<List<AgentCustomerDto>>>
    {
        private readonly IAgentPortalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetAgentMyCustomersQueryHandler(IAgentPortalQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<AgentCustomerDto>>> Handle(GetAgentMyCustomersQuery request, CancellationToken cancellationToken)
        {
            var partyId = _ipAddressService.GetPartyId();
            if (!partyId.HasValue)
                return new ApiResponseDTO<List<AgentCustomerDto>> { IsSuccess = false, Message = "Agent not identified." };

            var (data, totalCount) = await _queryRepository.GetMyCustomersAsync(partyId.Value, request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<AgentCustomerDto>>
            {
                IsSuccess = true, Message = "Success", Data = data,
                TotalCount = totalCount, PageNumber = request.PageNumber, PageSize = request.PageSize
            };
        }
    }

    public class GetAgentEnquiriesQueryHandler : IRequestHandler<GetAgentEnquiriesQuery, ApiResponseDTO<List<AgentEnquiryListDto>>>
    {
        private readonly IAgentPortalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetAgentEnquiriesQueryHandler(IAgentPortalQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<AgentEnquiryListDto>>> Handle(GetAgentEnquiriesQuery request, CancellationToken cancellationToken)
        {
            var partyId = _ipAddressService.GetPartyId();
            if (!partyId.HasValue)
                return new ApiResponseDTO<List<AgentEnquiryListDto>> { IsSuccess = false, Message = "Agent not identified." };

            var customerIds = await _queryRepository.GetAgentCustomerIdsAsync(partyId.Value);
            var (data, totalCount) = await _queryRepository.GetEnquiriesAsync(customerIds, request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<AgentEnquiryListDto>>
            {
                IsSuccess = true, Message = "Success", Data = data,
                TotalCount = totalCount, PageNumber = request.PageNumber, PageSize = request.PageSize
            };
        }
    }

    public class GetAgentSalesOrdersQueryHandler : IRequestHandler<GetAgentSalesOrdersQuery, ApiResponseDTO<List<AgentSalesOrderListDto>>>
    {
        private readonly IAgentPortalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetAgentSalesOrdersQueryHandler(IAgentPortalQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<AgentSalesOrderListDto>>> Handle(GetAgentSalesOrdersQuery request, CancellationToken cancellationToken)
        {
            var partyId = _ipAddressService.GetPartyId();
            if (!partyId.HasValue)
                return new ApiResponseDTO<List<AgentSalesOrderListDto>> { IsSuccess = false, Message = "Agent not identified." };

            var customerIds = await _queryRepository.GetAgentCustomerIdsAsync(partyId.Value);
            var (data, totalCount) = await _queryRepository.GetSalesOrdersAsync(customerIds, request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<AgentSalesOrderListDto>>
            {
                IsSuccess = true, Message = "Success", Data = data,
                TotalCount = totalCount, PageNumber = request.PageNumber, PageSize = request.PageSize
            };
        }
    }

    public class GetAgentComplaintsQueryHandler : IRequestHandler<GetAgentComplaintsQuery, ApiResponseDTO<List<AgentComplaintListDto>>>
    {
        private readonly IAgentPortalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetAgentComplaintsQueryHandler(IAgentPortalQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<AgentComplaintListDto>>> Handle(GetAgentComplaintsQuery request, CancellationToken cancellationToken)
        {
            var partyId = _ipAddressService.GetPartyId();
            if (!partyId.HasValue)
                return new ApiResponseDTO<List<AgentComplaintListDto>> { IsSuccess = false, Message = "Agent not identified." };

            var customerIds = await _queryRepository.GetAgentCustomerIdsAsync(partyId.Value);
            var (data, totalCount) = await _queryRepository.GetComplaintsAsync(customerIds, request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<AgentComplaintListDto>>
            {
                IsSuccess = true, Message = "Success", Data = data,
                TotalCount = totalCount, PageNumber = request.PageNumber, PageSize = request.PageSize
            };
        }
    }

    public class GetAgentInvoicesQueryHandler : IRequestHandler<GetAgentInvoicesQuery, ApiResponseDTO<List<AgentInvoiceListDto>>>
    {
        private readonly IAgentPortalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetAgentInvoicesQueryHandler(IAgentPortalQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<AgentInvoiceListDto>>> Handle(GetAgentInvoicesQuery request, CancellationToken cancellationToken)
        {
            var partyId = _ipAddressService.GetPartyId();
            if (!partyId.HasValue)
                return new ApiResponseDTO<List<AgentInvoiceListDto>> { IsSuccess = false, Message = "Agent not identified." };

            var customerIds = await _queryRepository.GetAgentCustomerIdsAsync(partyId.Value);
            var (data, totalCount) = await _queryRepository.GetInvoicesAsync(customerIds, request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<AgentInvoiceListDto>>
            {
                IsSuccess = true, Message = "Success", Data = data,
                TotalCount = totalCount, PageNumber = request.PageNumber, PageSize = request.PageSize
            };
        }
    }

    public class GetAgentDispatchesQueryHandler : IRequestHandler<GetAgentDispatchesQuery, ApiResponseDTO<List<AgentDispatchListDto>>>
    {
        private readonly IAgentPortalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetAgentDispatchesQueryHandler(IAgentPortalQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<AgentDispatchListDto>>> Handle(GetAgentDispatchesQuery request, CancellationToken cancellationToken)
        {
            var partyId = _ipAddressService.GetPartyId();
            if (!partyId.HasValue)
                return new ApiResponseDTO<List<AgentDispatchListDto>> { IsSuccess = false, Message = "Agent not identified." };

            var customerIds = await _queryRepository.GetAgentCustomerIdsAsync(partyId.Value);
            var (data, totalCount) = await _queryRepository.GetDispatchesAsync(customerIds, request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<AgentDispatchListDto>>
            {
                IsSuccess = true, Message = "Success", Data = data,
                TotalCount = totalCount, PageNumber = request.PageNumber, PageSize = request.PageSize
            };
        }
    }

    public class GetAgentCommissionsQueryHandler : IRequestHandler<GetAgentCommissionsQuery, ApiResponseDTO<List<AgentCommissionDto>>>
    {
        private readonly IAgentPortalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetAgentCommissionsQueryHandler(IAgentPortalQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<AgentCommissionDto>>> Handle(GetAgentCommissionsQuery request, CancellationToken cancellationToken)
        {
            var partyId = _ipAddressService.GetPartyId();
            if (!partyId.HasValue)
                return new ApiResponseDTO<List<AgentCommissionDto>> { IsSuccess = false, Message = "Agent not identified." };

            var data = await _queryRepository.GetCommissionsAsync(partyId.Value);

            return new ApiResponseDTO<List<AgentCommissionDto>> { IsSuccess = true, Message = "Success", Data = data };
        }
    }
}
