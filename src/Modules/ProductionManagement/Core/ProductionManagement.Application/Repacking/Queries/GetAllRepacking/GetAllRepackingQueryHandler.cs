using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.Repacking.Queries.GetAllRepacking
{
    public class GetAllRepackingQueryHandler : IRequestHandler<GetAllRepackingQuery, ApiResponseDTO<List<RepackingHeaderDto>>>
    {
        private readonly IRepackingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllRepackingQueryHandler(
            IRepackingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<RepackingHeaderDto>>> Handle(
            GetAllRepackingQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllRepackingQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Repacking details were fetched.",
                module: "Production"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<RepackingHeaderDto>>
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
