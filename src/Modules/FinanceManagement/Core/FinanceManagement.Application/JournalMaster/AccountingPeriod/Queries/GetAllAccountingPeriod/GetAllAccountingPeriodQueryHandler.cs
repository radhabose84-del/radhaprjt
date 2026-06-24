using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAllAccountingPeriod
{
    public class GetAllAccountingPeriodQueryHandler : IRequestHandler<GetAllAccountingPeriodQuery, ApiResponseDTO<List<AccountingPeriodDto>>>
    {
        private readonly IAccountingPeriodQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllAccountingPeriodQueryHandler(IAccountingPeriodQueryRepository queryRepository, IIPAddressService ipAddressService, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AccountingPeriodDto>>> Handle(GetAllAccountingPeriodQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, companyId, request.FinancialYearId);

            var dtos = _mapper.Map<List<AccountingPeriodDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllAccountingPeriodQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Accounting Period details were fetched.",
                module: "AccountingPeriod"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AccountingPeriodDto>>
            {
                IsSuccess = true,
                Message = "Accounting Period list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
