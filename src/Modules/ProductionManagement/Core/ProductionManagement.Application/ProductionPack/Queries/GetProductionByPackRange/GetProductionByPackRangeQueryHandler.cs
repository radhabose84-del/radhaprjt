using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProductionPack.Queries.GetProductionByPackRange
{
    public class GetProductionByPackRangeQueryHandler
        : IRequestHandler<GetProductionByPackRangeQuery, List<ProductionPackDetailDto>>
    {
        private readonly IProductionQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProductionByPackRangeQueryHandler(
            IProductionQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper          = mapper;
            _mediator        = mediator;
        }

        public async Task<List<ProductionPackDetailDto>> Handle(
            GetProductionByPackRangeQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByPackRangeAsync(
                request.StartPackNo, request.EndPackNo, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByPackRange",
                actionCode:   "GetProductionByPackRangeQuery",
                actionName:   $"{request.StartPackNo}-{request.EndPackNo}",
                details:      $"Production pack details fetched for pack range {request.StartPackNo} to {request.EndPackNo}.",
                module:       "ProductionPack"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return result;
        }
    }
}
