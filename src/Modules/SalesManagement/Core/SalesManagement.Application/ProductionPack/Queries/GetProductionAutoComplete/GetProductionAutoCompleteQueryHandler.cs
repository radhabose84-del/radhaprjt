using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProductionPack;
using SalesManagement.Application.ProductionPack.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProductionPack.Queries.GetProductionAutoComplete
{
    public class GetProductionAutoCompleteQueryHandler
        : IRequestHandler<GetProductionAutoCompleteQuery, IReadOnlyList<ProductionLookupDto>>
    {
        private readonly IProductionQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProductionAutoCompleteQueryHandler(
            IProductionQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ProductionLookupDto>> Handle(
            GetProductionAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(
                request.Term ?? string.Empty, cancellationToken);

            var dtos = _mapper.Map<List<ProductionLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetProductionAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Pack Allocation details was fetched.",
                module: "Production"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
