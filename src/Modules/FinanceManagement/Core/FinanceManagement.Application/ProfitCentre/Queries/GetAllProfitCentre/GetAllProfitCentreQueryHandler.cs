using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Queries.GetAllProfitCentre
{
    public class GetAllProfitCentreQueryHandler : IRequestHandler<GetAllProfitCentreQuery, ApiResponseDTO<List<ProfitCentreDto>>>
    {
        private readonly IProfitCentreQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllProfitCentreQueryHandler(
            IProfitCentreQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ProfitCentreDto>>> Handle(GetAllProfitCentreQuery request, CancellationToken cancellationToken)
        {
            // Profit Centres are group-level segments (code unique across companies), so the list is
            // not company/unit scoped.
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<ProfitCentreDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllProfitCentreQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "ProfitCentre details were fetched.",
                module: "ProfitCentre"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ProfitCentreDto>>
            {
                IsSuccess = true,
                Message = "Profit Centre list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
