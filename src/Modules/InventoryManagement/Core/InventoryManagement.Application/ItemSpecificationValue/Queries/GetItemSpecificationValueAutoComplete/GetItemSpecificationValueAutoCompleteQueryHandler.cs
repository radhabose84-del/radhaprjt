
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueAutoComplete
{
    public class GetItemSpecificationValueAutoCompleteQueryHandler : IRequestHandler<GetItemSpecificationValueAutoCompleteQuery, IReadOnlyList<ItemSpecificationValueLookupDto>>
    {
        private readonly IItemSpecificationValueQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetItemSpecificationValueAutoCompleteQueryHandler(
            IItemSpecificationValueQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ItemSpecificationValueLookupDto>> Handle(GetItemSpecificationValueAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<ItemSpecificationValueLookupDto>>(result);

            if (request.SpecificationMasterId.HasValue && request.SpecificationMasterId.Value > 0)
            {
                dtos = dtos.Where(x => x.SpecificationMasterId == request.SpecificationMasterId.Value).ToList();
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetItemSpecificationValueAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Item Specification Value details was fetched.",
                module: "ItemSpecificationValue"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
