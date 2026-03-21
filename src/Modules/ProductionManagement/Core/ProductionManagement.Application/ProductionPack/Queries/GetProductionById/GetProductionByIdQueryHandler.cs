using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProductionPack.Queries.GetProductionById
{
    public class GetProductionByIdQueryHandler
        : IRequestHandler<GetProductionByIdQuery, ProductionPackHeaderDto?>
    {
        private readonly IProductionQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProductionByIdQueryHandler(
            IProductionQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ProductionPackHeaderDto?> Handle(
            GetProductionByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetProductionByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Pack Allocation details {result.Id} was fetched.",
                module: "Production"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
