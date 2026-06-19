using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Queries.GetAllCostCentre
{
    public class GetAllCostCentreQueryHandler : IRequestHandler<GetAllCostCentreQuery, ApiResponseDTO<List<CostCentreDto>>>
    {
        private readonly ICostCentreQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllCostCentreQueryHandler(
            ICostCentreQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CostCentreDto>>> Handle(GetAllCostCentreQuery request, CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId()
                ?? throw new ExceptionRules("No active unit in session.");

            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, unitId);

            var dtos = _mapper.Map<List<CostCentreDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllCostCentreQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "CostCentre details were fetched.",
                module: "CostCentre"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<CostCentreDto>>
            {
                IsSuccess = true,
                Message = "Cost Centre list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
