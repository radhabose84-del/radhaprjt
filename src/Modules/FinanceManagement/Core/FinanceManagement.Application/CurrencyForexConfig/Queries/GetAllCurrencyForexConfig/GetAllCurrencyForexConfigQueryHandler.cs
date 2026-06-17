using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Queries.GetAllCurrencyForexConfig
{
    public class GetAllCurrencyForexConfigQueryHandler : IRequestHandler<GetAllCurrencyForexConfigQuery, ApiResponseDTO<List<CurrencyForexConfigDto>>>
    {
        private readonly ICurrencyForexConfigQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllCurrencyForexConfigQueryHandler(
            ICurrencyForexConfigQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CurrencyForexConfigDto>>> Handle(GetAllCurrencyForexConfigQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, companyId);

            var dtos = _mapper.Map<List<CurrencyForexConfigDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllCurrencyForexConfigQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "CurrencyForexConfig details were fetched.",
                module: "CurrencyForexConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<CurrencyForexConfigDto>>
            {
                IsSuccess = true,
                Message = "Currency Forex Config list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
