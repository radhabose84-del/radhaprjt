
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Queries.GetItemSpecificationMasterAutoComplete
{
    public class GetItemSpecificationMasterAutoCompleteQueryHandler : IRequestHandler<GetItemSpecificationMasterAutoCompleteQuery, IReadOnlyList<ItemSpecificationMasterLookupDto>>
    {
        private readonly IItemSpecificationMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetItemSpecificationMasterAutoCompleteQueryHandler(
            IItemSpecificationMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ItemSpecificationMasterLookupDto>> Handle(GetItemSpecificationMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<ItemSpecificationMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetItemSpecificationMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Item Specification Master details was fetched.",
                module: "ItemSpecificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
