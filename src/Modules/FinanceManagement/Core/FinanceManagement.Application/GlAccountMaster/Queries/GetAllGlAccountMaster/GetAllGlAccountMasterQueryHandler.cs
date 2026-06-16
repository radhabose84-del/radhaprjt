using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetAllGlAccountMaster
{
    public class GetAllGlAccountMasterQueryHandler : IRequestHandler<GetAllGlAccountMasterQuery, ApiResponseDTO<List<GlAccountMasterDto>>>
    {
        private readonly IGlAccountMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllGlAccountMasterQueryHandler(
            IGlAccountMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GlAccountMasterDto>>> Handle(GetAllGlAccountMasterQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm,
                companyId, request.AccountTypeId, request.AccountGroupId);

            var dtos = _mapper.Map<List<GlAccountMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllGlAccountMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "GlAccountMaster details were fetched.",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GlAccountMasterDto>>
            {
                IsSuccess = true,
                Message = "GL Account list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
