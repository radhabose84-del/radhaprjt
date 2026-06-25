using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Queries.GetAllFinancialYearMaster
{
    public class GetAllFinancialYearMasterQueryHandler : IRequestHandler<GetAllFinancialYearMasterQuery, ApiResponseDTO<List<FinancialYearMasterDto>>>
    {
        private readonly IFinancialYearMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllFinancialYearMasterQueryHandler(
            IFinancialYearMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<FinancialYearMasterDto>>> Handle(GetAllFinancialYearMasterQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm,
                companyId, request.StatusId);

            var dtos = _mapper.Map<List<FinancialYearMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllFinancialYearMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "FinancialYearMaster details were fetched.",
                module: "FinancialYearMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<FinancialYearMasterDto>>
            {
                IsSuccess = true,
                Message = "Financial Year list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
