using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Application.UsageType.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UsageType.Queries.GetUsageTypeAutoComplete
{
    public class GetUsageTypeAutoCompleteQueryHandler : IRequestHandler<GetUsageTypeAutoCompleteQuery, IReadOnlyList<UsageTypeLookupDto>>
    {
        private readonly IUsageTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetUsageTypeAutoCompleteQueryHandler(IUsageTypeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<UsageTypeLookupDto>> Handle(GetUsageTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<UsageTypeLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetUsageTypeAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "UsageType details was fetched.",
                module: "UsageType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
