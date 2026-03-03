using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProduction;
using SalesManagement.Application.Production.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Production.Queries.GetAllProduction
{
    public class GetAllProductionQueryHandler
        : IRequestHandler<GetAllProductionQuery, ApiResponseDTO<List<ProductionPackHeaderDto>>>
    {
        private readonly IProductionQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllProductionQueryHandler(
            IProductionQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ProductionPackHeaderDto>>> Handle(
            GetAllProductionQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllProductionQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Pack Allocation details were fetched.",
                module: "Production"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ProductionPackHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
